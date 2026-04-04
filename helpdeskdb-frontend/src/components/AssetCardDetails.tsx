import { IAssetViewModel } from "@/types/domain/IAssetViewModels";
import { useTranslation } from "react-i18next";

export function AssetCardDetails({ asset }: { asset: IAssetViewModel }) {
    const { t: tAssetViewModel } = useTranslation("assetviewmodel");
    const { t: tCommon } = useTranslation("common");

    return (
        <>
            <h3 className="font-bold text-xl text-gray-800 mb-4 break-words">
                {asset.assetName}
            </h3>
            <div className="space-y-3 text-gray-600">
                <div className="flex items-baseline gap-3">
                    <span className="font-medium text-gray-700 min-w-[120px]">
                        {tAssetViewModel("SerialNumber")}:
                    </span>
                    <span className="break-words flex-1">{asset.serialNumber || "-"}</span>
                </div>
                <div className="flex items-baseline gap-3">
                    <span className="font-medium text-gray-700 min-w-[120px]">
                        {tAssetViewModel("Category")}:
                    </span>
                    <span className="break-words flex-1">{asset.categoryName}</span>
                </div>
                <div className="flex items-baseline gap-3">
                    <span className="font-medium text-gray-700 min-w-[120px]">
                        {tAssetViewModel("Owner")}:
                    </span>
                    <span className="break-words flex-1">{asset.ownerName}</span>
                </div>
                <div className="flex items-baseline gap-3">
                    <span className="font-medium text-gray-700 min-w-[120px]">
                        {tAssetViewModel("Location")}:
                    </span>
                    <span className="break-words flex-1">
                        {asset.roomName} {asset.cupboardName}
                    </span>
                </div>
                <div className="flex items-baseline gap-3">
                    <span className="font-medium text-gray-700 min-w-[120px]">
                        {tAssetViewModel("ShelfNum")}:
                    </span>
                    <span className="break-words flex-1">{asset.shelfNum}</span>
                </div>
                <div className="flex items-baseline gap-3">
                    <span className="font-medium text-gray-700 min-w-[120px]">
                        {tAssetViewModel("Column")}:
                    </span>
                    <span className="break-words flex-1">{asset.column}</span>
                </div>
                <div className="flex items-baseline gap-3">
                    <span className="font-medium text-gray-700 min-w-[120px]">
                        {tAssetViewModel("ClosestReservationBy")}:
                    </span>
                    <span className="break-words flex-1">{asset.closestReservationBy}</span>
                </div>
                <div className="flex items-baseline gap-3">
                    <span className="font-medium text-gray-700 min-w-[120px]">
                        {tCommon("AddedAt")}:
                    </span>
                    <span className="break-words flex-1">
                        {new Date(asset.addedAt).toLocaleDateString()}
                    </span>
                </div>
            </div>
        </>
    );
}
