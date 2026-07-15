import { IAccountInfo } from "@/context/AccountContext";
import { IIdentityResponse } from "@/types/IIdentityResponse";
import axios, { AxiosInstance } from "axios";

// Shared across every service instance so concurrent 401s trigger only ONE
// refresh-token rotation. Without this, two services hitting a 401 at the same
// time both call renewRefreshToken, the backend rotates the hd_rt cookie on the
// first, and the second presents an already-invalidated token, fails, and logs
// the user out.
let sharedRefresh: Promise<IIdentityResponse | null> | null = null;

export abstract class BaseService {
	protected axiosInstance: AxiosInstance;

	private setAccountInfo?: (value: IAccountInfo) => void;

	public injectSetAccountInfo(fn: (value: IAccountInfo) => void) {
		this.setAccountInfo = fn;
	}

	constructor() {
		this.axiosInstance = axios.create({
			baseURL: "/api/v1/",
			headers: {
				"Content-Type": "application/json",
				Accept: "application/json",
			},
			withCredentials: true,
		});

		this.axiosInstance.interceptors.response.use(
			(response) => response,
			async (error) => {
				const originalRequest = error.config;
				if (
					error.response &&
					error.response.status === 401 &&
					!originalRequest._retry &&
					!originalRequest.url?.includes("account/renewRefreshToken") &&
					!originalRequest.url?.includes("account/login")
				) {
					originalRequest._retry = true;
					try {
						// Coalesce concurrent refreshes onto a single in-flight call.
						if (!sharedRefresh) {
							sharedRefresh = this.axiosInstance
								.post<IIdentityResponse>(
									"account/renewRefreshToken",
									null
								)
								// axios rejects non-2xx responses, so reaching .then means success
								.then((response) => response.data)
								.finally(() => {
									sharedRefresh = null;
								});
						}

						const identity = await sharedRefresh;

						if (identity) {
							this.setAccountInfo?.({
								id: identity.id,
								name: identity.username,
								roles: identity.roles,
							});
							return this.axiosInstance(originalRequest);
						}

						return Promise.reject(error);
					} catch (refreshError) {
						this.setAccountInfo?.({});
						if (typeof window !== "undefined" && !window.location.pathname.startsWith("/login")) {
							window.location.href = "/login";
						}
						return Promise.reject(refreshError);
					}
				}
				return Promise.reject(error);
			}
		);
	}
}
