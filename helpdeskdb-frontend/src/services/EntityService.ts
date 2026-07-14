import { IResultObject } from "@/types/IResultObject";
import { BaseService } from "./BaseService";
import { AxiosError, AxiosResponse } from "axios";
import { IDomainId } from "@/types/IDomainId";

export abstract class EntityService<
	TEntity extends IDomainId,
	TAddEntity,
> extends BaseService {
	constructor(protected basePath: string) {
		super();
	}

	async getAllAsync(): Promise<IResultObject<TEntity[]>> {
		try {
			const response = await this.axiosInstance.get<TEntity[]>(
				this.basePath,
			);

			console.log("getAll response", response);

			if (response.status < 300) {
				return {
					statusCode: response.status,
					data: response.data,
				};
			}

			return {
				statusCode: response.status,
				errors: [
					(
						response.status.toString() +
						" " +
						response.statusText
					).trim(),
				],
			};
		} catch (error) {
			return this.handleError(error);
		}
	}

	async getAsync(id: string): Promise<IResultObject<TEntity>> {
		try {
			const response = await this.axiosInstance.get<TEntity>(
				this.basePath + "/" + id,
			);

			console.log("get response", response);

			if (response.status < 300) {
				return {
					statusCode: response.status,
					data: response.data,
				};
			}

			return {
				statusCode: response.status,
				errors: [
					(
						response.status.toString() +
						" " +
						response.statusText
					).trim(),
				],
			};
		} catch (error) {
			return this.handleError(error);
		}
	}

	async deleteAsync(id: string): Promise<IResultObject<null>> {
		try {
			const response = await this.axiosInstance.delete<null>(
				this.basePath + "/" + id,
			);

			console.log("get response", response);

			if (response.status < 300) {
				return {
					statusCode: response.status,
					data: null,
				};
			}

			return {
				statusCode: response.status,
				errors: [
					(
						response.status.toString() +
						" " +
						response.statusText
					).trim(),
				],
			};
		} catch (error) {
			return this.handleError(error);
		}
	}

	async addAsync(entity: TAddEntity): Promise<IResultObject<TEntity>> {
		try {
			const response = await this.axiosInstance.post<TEntity>(
				this.basePath,
				entity,
			);

			console.log("login response", response);

			if (response.status < 300) {
				return {
					statusCode: response.status,
					data: response.data,
				};
			}

			return {
				statusCode: response.status,
				errors: [
					(
						response.status.toString() +
						" " +
						response.statusText
					).trim(),
				],
			};
		} catch (error) {
			return this.handleError(error);
		}
	}

	async updateAsync(entity: TEntity): Promise<IResultObject<TEntity>> {
		try {
			const response = await this.axiosInstance.put<TEntity>(
				this.basePath + "/" + entity.id,
				entity,
			);

			console.log("login response", response);

			if (response.status < 300) {
				return {
					statusCode: response.status,
					data: response.data,
				};
			}

			return {
				statusCode: response.status,
				errors: [
					(
						response.status.toString() +
						" " +
						response.statusText
					).trim(),
				],
			};
		} catch (error) {
			return this.handleError(error);
		}
	}

	protected handleError<T>(error: unknown): IResultObject<T> {
		const axiosErr = error as AxiosError;
		console.log(
			"EntityService error:",
			axiosErr.message,
			axiosErr.response?.data,
		);

		const response = axiosErr.response as
			| AxiosResponse<{
					messages?: string[];
					message?: string;
					Message?: string;
			  }>
			| undefined;
		const serverMessages = response?.data?.messages;
		const serverMessage =
			response?.data?.Message || response?.data?.message;

		return {
			statusCode: response?.status ?? axiosErr.status ?? 0,
			errors: serverMessages?.length
				? serverMessages
				: serverMessage
					? [serverMessage]
					: [axiosErr.message ?? axiosErr.code ?? "UNKNOWN_ERROR"],
		};
	}
}
