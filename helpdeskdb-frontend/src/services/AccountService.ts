import { AxiosError } from "axios"
import { BaseService } from "./BaseService"
import { IResultObject } from "@/types/IResultObject"
import { IIdentityResponse } from "@/types/IIdentityResponse"

export class AccountService extends BaseService {

	async logoutAsync(): Promise<void> {
		try {
			await this.axiosInstance.post('account/logout', null);
		} catch (error) {
			console.log('logout error:', (error as Error).message);
		}
	}

	async loginAsync(username: string, password: string): Promise<IResultObject<IIdentityResponse>> {
		const url = 'account/login'
		try {
			const loginData = { username, password }

			const response = await this.axiosInstance.post<IIdentityResponse>(
				url,
				loginData
			)

			if (response.status < 300) {
				return {
					statusCode: response.status,
					data: response.data
				}
			}
			return {
				statusCode: response.status,
				errors: [(response.status.toString() + ' ' + response.statusText).trim()],
			}
		} catch (error) {
			console.log('login error: ', (error as Error).message)
			return {
				statusCode: (error as AxiosError)?.status,
				errors: [(error as AxiosError).code ?? ""],
			}
		}
	}

	async meAsync(): Promise<IResultObject<IIdentityResponse>> {
		try {
			const response = await this.axiosInstance.get<IIdentityResponse>('account/me')
			if (response.status < 300) {
				return {
					statusCode: response.status,
					data: response.data
				}
			}
			return {
				statusCode: response.status,
				errors: [(response.status.toString() + ' ' + response.statusText).trim()],
			}
		} catch (error) {
			return {
				statusCode: (error as AxiosError)?.response?.status ?? (error as AxiosError)?.status,
				errors: [(error as AxiosError).code ?? ""],
			}
		}
	}
}
