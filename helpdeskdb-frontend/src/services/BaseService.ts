import { getRolesFromToken, getUserIdFromToken, getUsernameFromToken } from "@/helpers/JwtHelper";
import { ILoginDto } from "@/types/ILoginDto";
import axios, { AxiosInstance } from "axios";
import { jwtDecode } from "jwt-decode";
export abstract class BaseService {
	protected axiosInstance: AxiosInstance;

	// private setAccountInfo = useContext(AccountContext).setAccountInfo;
	private setAccountInfo?: (value: { jwt: string; refreshToken: string; roles: string[]; name: string; id: string }) => void;

	public injectSetAccountInfo(fn: (value: { jwt: string; refreshToken: string; roles: string[]; name: string; id: string }) => void) {
		this.setAccountInfo = fn;
	}


	constructor() {
		this.axiosInstance
			= axios.create({
				baseURL: "/api/v1/",
				headers: {
					"Content-Type": "application/json",
					Accept: "application/json",
				},
			});


		this.axiosInstance.interceptors.request.use(
			async (config) => {
				const jwt = localStorage.getItem("_jwt");
				if (jwt) {
					let tokenToUse = jwt;
					try {
						const decoded = jwtDecode<{ exp: number }>(jwt);
						const now = Math.floor(Date.now() / 1000);
						if (decoded.exp - now < 2) {
							const refreshToken = localStorage.getItem("_refreshToken");
							if (refreshToken) {
								try {
									const response = await axios.post<ILoginDto>(
										"/api/v1/account/renewRefreshToken",
										{ jwt, refreshToken }
									);
									if (response.status < 300) {
										localStorage.setItem("_jwt", response.data.jwt);
										localStorage.setItem("_refreshToken", response.data.refreshToken);
										this.setAccountInfo?.({
											jwt: response.data.jwt,
											refreshToken: response.data.refreshToken,
											roles: getRolesFromToken(response.data.jwt),
											name: getUsernameFromToken(response.data.jwt),
											id: getUserIdFromToken(response.data.jwt),
										});
										tokenToUse = response.data.jwt;
									}
								} catch {
									// Renewal failed (refresh token expired) — redirect to login
									localStorage.removeItem("_jwt");
									localStorage.removeItem("_refreshToken");
									window.location.href = "/login";
									return Promise.reject(new Error("Session expired"));
								}
							}
						}
					} catch {
						// jwtDecode failed — proceed with existing token
					}
					config.headers.Authorization = `Bearer ${tokenToUse}`;
				}
				return config;
			},
			(error) => {
				return Promise.reject(error);
			}
		);

		this.axiosInstance.interceptors.response.use(
			(response) => {
				return response;
			},

			async (error) => {
				const oringinalRequest = error.config;
				if (error.response && error.response.status === 401 && !oringinalRequest._retry) {
					oringinalRequest._retry = true;
					try {
						const jwt = localStorage.getItem("_jwt");
						const refreshToken = localStorage.getItem("_refreshToken");

						if (!jwt || !refreshToken) {
							return Promise.reject(error);
						}

						const response = await axios.post<ILoginDto>(
							"/api/v1/account/renewRefreshToken?jwtExpiresInSeconds=5",
							{
								jwt: jwt,
								refreshToken: refreshToken,
							}
						);

						console.log("renewRefreshToken", response);

						if (response && response.status <= 300) {
							localStorage.setItem("_jwt", response.data.jwt);
							localStorage.setItem("_refreshToken", response.data.refreshToken);
							oringinalRequest.headers.Authorization = `Bearer ${response.data.jwt}`;

							this.setAccountInfo?.({
								jwt: response.data.jwt,
								refreshToken: response.data.refreshToken,
								roles: getRolesFromToken(response.data.jwt),
								name: getUsernameFromToken(response.data.jwt),
								id: getUserIdFromToken(response.data.jwt)
							});

							return this.axiosInstance(oringinalRequest);
						}

						return Promise.reject(error);
					} catch (refreshError) {
						// Renewal failed (refresh token expired) — redirect to login
						localStorage.removeItem("_jwt");
						localStorage.removeItem("_refreshToken");
						window.location.href = "/login";
						return Promise.reject(refreshError);
					}

				}
				return Promise.reject(error);
			}
		);
	}

	/**
	 * Checks if JWT is expired and refreshes it if needed.
	 * Returns true if token is valid (refreshed or still valid), false otherwise.
	 */
	public async refreshTokenIfNeeded(): Promise<boolean> {
		const jwt = localStorage.getItem("_jwt");
		const refreshToken = localStorage.getItem("_refreshToken");
		if (!jwt || !refreshToken) return false;

		// Decode JWT to check expiration
		try {
			const decoded = jwtDecode<{ exp: number }>(jwt);
			const now = Math.floor(Date.now() / 1000);
			// If token expires in less than 2 seconds, refresh it
			if (decoded.exp - now < 2) {
				const response = await axios.post<ILoginDto>(
					"/api/v1/account/renewRefreshToken?jwtExpiresInSeconds=5",
					{ jwt, refreshToken }
				);
				if (response && response.status <= 300) {
					localStorage.setItem("_jwt", response.data.jwt);
					localStorage.setItem("_refreshToken", response.data.refreshToken);
					if (this.setAccountInfo) {
						this.setAccountInfo({
							jwt: response.data.jwt,
							refreshToken: response.data.refreshToken,
							roles: getRolesFromToken(response.data.jwt),
							name: getUsernameFromToken(response.data.jwt),
							id: getUserIdFromToken(response.data.jwt)
						});
					}
					return true;
				}
				return false;
			}
			return true; // Token is still valid
		} catch {
			return false;
		}
	}
}
