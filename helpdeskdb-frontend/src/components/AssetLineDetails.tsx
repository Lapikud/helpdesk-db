import { IAssetViewModel } from "@/types/domain/IAssetViewModels";

export function AssetLineDetails({ asset }: { asset: IAssetViewModel }) {
	return (
		<>
			<div className="p-2 text-center truncate hover:whitespace-normal">
				{asset.assetName}
			</div>
			<div className="p-2 text-center truncate hover:whitespace-normal">
				{asset.serialNumber || "-"}
			</div>
			<div className="p-2 text-center truncate hover:whitespace-normal">
				{asset.categoryName}
			</div>
			<div className="p-2 text-center truncate hover:whitespace-normal">
				{asset.ownerName}
			</div>
			<div className="p-2 text-center truncate hover:whitespace-normal">
				{asset.roomName}
			</div>
			<div className="p-2 text-center truncate hover:whitespace-normal">
				{asset.cupboardName}
			</div>
			<div className="p-2 text-center truncate hover:whitespace-normal">
				{asset.shelfNum}
			</div>
			<div className="p-2 text-center truncate hover:whitespace-normal">
				{asset.column}
			</div>
			<div className="p-2 text-center truncate hover:whitespace-normal">
				{asset.closestReservationBy}
			</div>
			<div className="p-2 text-center truncate hover:whitespace-normal">
				{new Date(asset.addedAt).toLocaleDateString()}
			</div>
		</>
	);
}
