import { IAsset, IAssetAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";
import { IResultObject } from "@/types/IResultObject";

export class AssetService extends EntityService<IAsset, IAssetAdd> {
	constructor() {
		super("assets");
	}

	async getAllAsync(
		includeRemoved: boolean = false,
	): Promise<IResultObject<IAsset[]>> {
		try {
			const response = await this.axiosInstance.get<IAsset[]>(
				this.basePath,
				{
					params: { includeRemoved },
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
	async getAssetNameById(assetId: string): Promise<string> {
		try {
			const response = await this.getAsync(assetId);
			if (!response.data) {
				console.error("Error getting asset:", response.errors);
				return "Unknown Asset";
			}
			return response.data.assetName;
		} catch (error) {
			console.error("Error fetching asset name:", error);
			return "Unknown Asset";
		}
	}
}
