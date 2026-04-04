import { AxiosError } from "axios";
import { BaseService } from "./BaseService";

import { IResultObject } from "@/types/IResultObject";
import { IUser } from "@/types/domain/DomainTypes";

export class UserService extends BaseService {
	constructor() {
		super();
	}

	async getAllAsync(): Promise<IResultObject<IUser[]>> {
		try {

			const response = await this.axiosInstance.get<IUser[]>("users")

			console.log('getAll response', response)

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
			return this.handleError(error);
		}
	}

	async getAsync(id: string): Promise<IResultObject<IUser>> {
		try {

			const response = await this.axiosInstance.get<IUser>("users" + "/" + id)

			console.log('get response', response)

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
			return this.handleError(error);
		}
	}

	private handleError<T>(error: unknown): IResultObject<T> {
		const axiosErr = error as AxiosError;
		console.log("UserService error:", axiosErr.message);

		return {
			statusCode: axiosErr.status ?? 0,
			errors: [axiosErr.code ?? "UNKNOWN_ERROR"],
		};
	}
}
