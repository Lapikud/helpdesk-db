import { IDomainId } from "../IDomainId";

export interface IAssetViewModel extends IDomainId {
	assetName: string;
	serialNumber: string | null;
	barcode: string | null;
	categoryName: string;
	ownerName: string;
	roomName: string;
	cupboardName: string;
	shelfNum: number;
	column: number;
	closestReservationBy: string;
	addedAt: string; // ISO string
	reservationId: string | null;
	reservationTo: string | null;
	reserved: boolean;
}

export interface IAssetsOverviewViewModel {
	availableAssets: IAssetViewModel[];
	assetsReservedByUser: IAssetViewModel[];
}

export interface IAssetViewModelCreate {
	assetName: string;
	comment: string;
	serialNumber?: string | null;
	barcode?: string | null;
	selectedCategoryId: string;
	selectedOwnerId: string;
	selectedLocationId: string;
}

export interface IAssetViewModelUpdate {
	assetId: string;
	assetName: string;
	comment: string;
	serialNumber?: string | null;
	barcode?: string | null;
	selectedCategoryId: string;
	selectedOwnerId: string;
	selectedLocationId: string;
	categoryAssetsId: string | null;
	ownerAssetsId: string | null;
	locationAssetsId: string | null;
}

export interface IAssetViewModelRemove {
	assetId: string;
	comment: string;
}
