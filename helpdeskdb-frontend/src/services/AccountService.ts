import { AxiosError } from "axios"
import { BaseService } from "./BaseService"
import { IResultObject } from "@/types/IResultObject"
import { ILoginDto } from "@/types/ILoginDto"

export class AccountService extends BaseService {

	async logoutAsync(refreshToken: string): Promise<void> {
		try {
			await this.axiosInstance.post('account/logout', { refreshToken });
		} catch (error) {
			console.log('logout error:', (error as Error).message);
		}
	}

	async loginAsync(username: string, password: string): Promise<IResultObject<ILoginDto>> {
		const url = 'account/login'
		try {
			const loginData = {
				username,
				password,
			}

			const response = await this.axiosInstance.post<ILoginDto>(url + "?jwtExpiresInSeconds=5", loginData)

			console.log('login response', response)

			if (response.status <= 300) {
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
			console.log('error: ', (error as Error).message)
			return {
				statusCode: (error as AxiosError)?.status,
				errors: [(error as AxiosError).code ?? ""],
			}
		}
	}
}
