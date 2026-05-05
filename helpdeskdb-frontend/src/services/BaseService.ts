import { IAccountInfo } from "@/context/AccountContext";
import { IIdentityResponse } from "@/types/IIdentityResponse";
import axios, { AxiosInstance } from "axios";

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
						const response = await this.axiosInstance.post<IIdentityResponse>(
							"account/renewRefreshToken?jwtExpiresInSeconds=5",
							null
						);

						if (response.status < 300) {
							this.setAccountInfo?.({
								id: response.data.id,
								name: response.data.username,
								roles: response.data.roles,
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
