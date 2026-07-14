import { AxiosError } from "axios";
import type { AxiosResponse } from "axios";
import { BaseService } from "./BaseService";
import {
	IAssetViewModel,
	IAssetsOverviewViewModel,
	IAssetViewModelCreate,
	IAssetViewModelUpdate,
	IAssetViewModelRemove,
} from "@/types/domain/IAssetViewModels";
import { IResultObject } from "@/types/IResultObject";
import {
	IAssetReservationAdd,
	IAssetReservationUpdate,
} from "@/types/domain/DomainTypes";

export class OverviewService extends BaseService {
	constructor() {
		super();
	}

	async getOverview(
		searchTerm?: string,
	): Promise<IResultObject<IAssetsOverviewViewModel>> {
		try {
			const response =
				await this.axiosInstance.get<IAssetsOverviewViewModel>(
					"home/overview",
					{
						params: searchTerm ? { searchTerm } : {},
					},
				);

			if (response.status < 300) {
				return { statusCode: response.status, data: response.data };
			}

			return {
				statusCode: response.status,
				errors: [`${response.status} ${response.statusText}`.trim()],
			};
		} catch (error) {
			return this.handleError(error);
		}
	}

	async createAsset(
		data: IAssetViewModelCreate,
	): Promise<IResultObject<IAssetViewModel>> {
		try {
			const response = await this.axiosInstance.post<IAssetViewModel>(
				"home/overview/createNewAsset",
				data,
			);

			if (response.status < 300) {
				return { statusCode: response.status, data: response.data };
			}

			return {
				statusCode: response.status,
				errors: [`${response.status} ${response.statusText}`.trim()],
			};
		} catch (error) {
			return this.handleError(error);
		}
	}

	async updateAsset(
		id: string,
		data: IAssetViewModelUpdate,
	): Promise<IResultObject<null>> {
		try {
			const response = await this.axiosInstance.put(
				`home/overview/edit/${id}`,
				data,
			);

			if (response.status < 300) {
				return { statusCode: response.status, data: null };
			}

			return {
				statusCode: response.status,
				errors: [`${response.status} ${response.statusText}`.trim()],
			};
		} catch (error) {
			return this.handleError(error);
		}
	}

	async removeAsset(
		id: string,
		data: IAssetViewModelRemove,
	): Promise<IResultObject<{ message: string }>> {
		try {
			const response = await this.axiosInstance.post<{ message: string }>(
				`home/overview/remove/${id}`,
				data,
			);

			if (response.status < 300) {
				return { statusCode: response.status, data: response.data };
			}

			return {
				statusCode: response.status,
				errors: [`${response.status} ${response.statusText}`.trim()],
			};
		} catch (error) {
			return this.handleError(error);
		}
	}

	async reserveAsset(
		id: string,
		data: IAssetReservationAdd,
	): Promise<IResultObject<{ message: string }>> {
		console.log(
			"data: " +
				data.assetId +
				", " +
				data.userId +
				", " +
				data.reservationFrom +
				", " +
				data.reservationTo,
		);
		try {
			const response = await this.axiosInstance.post<{ message: string }>(
				`home/overview/reserve/${id}`,
				data,
			);

			if (response.status < 300) {
				return { statusCode: response.status, data: response.data };
			}

			return {
				statusCode: response.status,
				errors: [`${response.status} ${response.statusText}`.trim()],
			};
		} catch (error) {
			return this.handleError(error);
		}
	}

	async changeReservationTime(
		reservationId: string,
		data: IAssetReservationUpdate,
	): Promise<IResultObject<null>> {
		try {
			const response = await this.axiosInstance.put(
				`home/overview/changeReservationTime/${reservationId}`,
				data,
			);

			if (response.status < 300) {
				return { statusCode: response.status, data: null };
			}

			return {
				statusCode: response.status,
				errors: [`${response.status} ${response.statusText}`.trim()],
			};
		} catch (error) {
			return this.handleError(error);
		}
	}

	async removeReservation(
		id: string,
	): Promise<IResultObject<{ message: string }>> {
		try {
			const response = await this.axiosInstance.post<{ message: string }>(
				`home/overview/remove-reservation/${id}`,
			);

			if (response.status < 300) {
				return { statusCode: response.status, data: response.data };
			}

			return {
				statusCode: response.status,
				errors: [`${response.status} ${response.statusText}`.trim()],
			};
		} catch (error) {
			return this.handleError(error);
		}
	}

	async returnAsset(
		id: string,
	): Promise<IResultObject<{ message: string }>> {
		try {
			const response = await this.axiosInstance.post<{ message: string }>(
				`home/overview/return/${id}`,
			);

			if (response.status < 300) {
				return { statusCode: response.status, data: response.data };
			}

			return {
				statusCode: response.status,
				errors: [`${response.status} ${response.statusText}`.trim()],
			};
		} catch (error) {
			return this.handleError(error);
		}
	}

	private handleError<T>(error: unknown): IResultObject<T> {
		const axiosErr = error as AxiosError;
		console.log(
			"OverviewService error:",
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
