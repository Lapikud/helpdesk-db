import { IAssetViewModel } from "@/types/domain/IAssetViewModels";
import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { useContext, useState } from "react";
import { AssetCardDetails } from "./AssetCardDetails";
import { AssetLineDetails } from "./AssetLineDetails";

interface AssetListProps {
	availableAssets: IAssetViewModel[];
	assetsReservedByUser: IAssetViewModel[];
	onEditAsset: (asset: IAssetViewModel) => Promise<void>;
	onRemoveAsset: (asset: IAssetViewModel) => Promise<void>;
	onReserveAsset: (asset: IAssetViewModel) => Promise<void>;
	onChangeReservationTime: (reservationId: string) => Promise<void>;
	onRemoveReservation: (assetId: string) => Promise<void>;
	onReturnAsset: (assetId: string) => Promise<void>;
	loading: Record<string, boolean>;
	mode: string;
}

export default function AssetList({
	availableAssets,
	assetsReservedByUser,
	onEditAsset,
	onRemoveAsset,
	onReserveAsset,
	onChangeReservationTime,
	onRemoveReservation,
	onReturnAsset,
	loading,
	mode,
}: AssetListProps) {
	const { t: tAssetlistpartial } = useTranslation("_assetlistpartial");
	const { t: tCommon } = useTranslation("common");
	const { t: tAssetViewModel } = useTranslation("assetviewmodel");

	const { accountInfo } = useContext(AccountContext);

	const isAdmin = accountInfo?.roles?.includes("admins") ?? false;
	const isMember = accountInfo?.roles?.includes("members") ?? false;

	function getInitialSections() {
		if (typeof window !== "undefined") {
			const stored = localStorage.getItem("assetListOpenSections");
			if (stored) return JSON.parse(stored);
		}
		return { availableAssets: true, assetsReservedByUser: true };
	}

	type SectionState = {
		availableAssets: boolean;
		assetsReservedByUser: boolean;
	};

	const [openSections, setOpenSections] =
		useState<SectionState>(getInitialSections);

	const handleToggleSection = (
		section: "availableAssets" | "assetsReservedByUser",
	) => {
		setOpenSections((prev: SectionState) => {
			const updated = { ...prev, [section]: !prev[section] };
			localStorage.setItem(
				"assetListOpenSections",
				JSON.stringify(updated),
			);
			return updated;
		});
	};

	const renderTableHeader = (showActions: boolean) => {
		return (
			<div
				className={`${
					showActions ? "grid-cols-11" : "grid-cols-10"
				} grid bg-gray-800 text-white font-semibold text-sm`}
			>
				<div className="p-3 text-center">
					{tAssetViewModel("AssetName")}
				</div>
				<div className="p-3 text-center">
					{tAssetViewModel("SerialNumber")}
				</div>
				<div className="p-3 text-center">
					{tAssetViewModel("Category")}
				</div>
				<div className="p-3 text-center">
					{tAssetViewModel("Owner")}
				</div>
				<div className="p-3 text-center">
					{tAssetViewModel("RoomName")}
				</div>
				<div className="p-3 text-center">
					{tAssetViewModel("CupboardName")}
				</div>
				<div className="p-3 text-center">
					{tAssetViewModel("ShelfNum")}
				</div>
				<div className="p-3 text-center">
					{tAssetViewModel("Column")}
				</div>
				<div className="p-3 text-center truncate hover:whitespace-normal">
					{tAssetViewModel("ClosestReservationBy")}
				</div>
				<div className="p-3 text-center">{tCommon("AddedAt")}</div>
				{showActions && (
					<div className="p-3 text-center">
						{tAssetlistpartial("Actions")}
					</div>
				)}
			</div>
		);
	};

	const isExpiredNotReturned = (asset: IAssetViewModel, isReserved: boolean) =>
		isReserved &&
		asset.reservationTo != null &&
		new Date(asset.reservationTo) < new Date();

	const renderAssets = (assets: IAssetViewModel[], isReserved: boolean) => {
		if (assets.length === 0) {
			return (
				<div className="text-center py-8 text-gray-500">
					{tAssetlistpartial("NoAssetsToDisplay")}
				</div>
			);
		}

		if (mode === "cards") {
			return (
				<div className="overflow-y-auto max-h-96">
				<ul className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
					{assets.map((asset) => (
						<li
							key={asset.id}
							className={`rounded-lg shadow-md overflow-hidden border hover:shadow-lg transition-shadow duration-200 ${
								isExpiredNotReturned(asset, isReserved)
									? "bg-red-50 border-red-300"
									: "bg-white border-gray-200"
							}`}
						>
							<div className="p-4">
								<AssetCardDetails asset={asset} />
								{(isAdmin || isMember) && (
									<div className="mt-5 pt-4 border-t border-gray-100 flex flex-wrap gap-3 justify-center">
										{isReserved ? (
											<>
												<button
													onClick={() =>
														onChangeReservationTime(
															asset.reservationId!,
														)
													}
													disabled={
														loading[asset.id] ||
														!asset.reservationId
													}
													className={`w-full bg-orange-500 hover:bg-orange-600 text-white font-medium py-2 px-4 rounded transition-colors duration-200 whitespace-nowrap ${
														loading[asset.id] ||
														!asset.reservationId
															? "opacity-50 cursor-not-allowed"
															: ""
													}`}
												>
													{tAssetlistpartial("ChangeReservationTime")}
												</button>
												<button
													onClick={() =>
														onReturnAsset(asset.id)
													}
													disabled={loading[asset.id]}
													className={`w-full bg-orange-500 hover:bg-orange-600 text-white font-medium py-2 px-4 rounded transition-colors duration-200 whitespace-nowrap ${
														loading[asset.id]
															? "opacity-50 cursor-not-allowed"
															: ""
													}`}
												>
													{tAssetlistpartial("Returned")}
												</button>
												<button
													onClick={() =>
														onRemoveReservation(
															asset.id,
														)
													}
													disabled={loading[asset.id]}
													className={`w-full bg-orange-500 hover:bg-orange-600 text-white font-medium py-2 px-4 rounded transition-colors duration-200 whitespace-nowrap ${
														loading[asset.id]
															? "opacity-50 cursor-not-allowed"
															: ""
													}`}
												>
													{tAssetlistpartial("RemoveReservation")}
												</button>
											</>
										) : asset.reserved ? (
											<span className="w-full text-center text-sm text-red-500 font-medium py-2 px-4">
												{tAssetlistpartial("NotYetReturned")}
											</span>
										) : (
											<button
												onClick={() =>
													onReserveAsset(asset)
												}
												disabled={loading[asset.id]}
												className={`w-full bg-orange-500 hover:bg-orange-600 text-white font-medium py-2 px-4 rounded transition-colors duration-200 whitespace-nowrap ${
													loading[asset.id]
														? "opacity-50 cursor-not-allowed"
														: ""
												}`}
											>
												{tAssetlistpartial("Reserve")}
											</button>
										)}
										<button
											onClick={() => onEditAsset(asset)}
											disabled={loading[asset.id]}
											className={`w-full sm:w-auto text-center bg-blue-50 hover:bg-blue-100 text-blue-600 font-medium py-2 px-4 rounded transition-colors duration-200 whitespace-nowrap ${
												loading[asset.id]
													? "opacity-50 cursor-not-allowed"
													: ""
											}`}
										>
											{tCommon("EditLink")}
										</button>
										<button
											onClick={() => onRemoveAsset(asset)}
											disabled={loading[asset.id]}
											className={`w-full sm:w-auto text-center bg-red-50 hover:bg-red-100 text-red-600 font-medium py-2 px-4 rounded transition-colors duration-200 whitespace-nowrap ${
												loading[asset.id]
													? "opacity-50 cursor-not-allowed"
													: ""
											}`}
										>
											{tAssetlistpartial("Remove")}
										</button>
									</div>
								)}
							</div>
						</li>
					))}
				</ul>
				</div>
			);
		} else {
			// "lines" mode
			return (
				<div className="rounded border overflow-hidden">
					{renderTableHeader(isAdmin || isMember)}
					<div className="overflow-y-auto max-h-64">
					{assets.map((asset) => (
						<div
							key={asset.id}
							className={`${
								isAdmin || isMember
									? "grid-cols-11"
									: "grid-cols-10"
							} grid text-sm text-center border-b last:rounded-b text-black ${
								isExpiredNotReturned(asset, isReserved)
									? "bg-red-50 hover:bg-red-100"
									: "hover:bg-gray-50"
							}`}
						>
							<AssetLineDetails asset={asset} />
							{(isAdmin || isMember) && (
								<div className="flex flex-wrap justify-center gap-2 p-2">
									{isReserved ? (
										<>
											<button
												onClick={() =>
													onChangeReservationTime(
														asset.reservationId!,
													)
												}
												disabled={
													loading[asset.id] ||
													!asset.reservationId
												}
												className={`bg-orange-500 hover:bg-orange-600 text-white font-medium py-1 px-3 rounded ${
													loading[asset.id] ||
													!asset.reservationId
														? "opacity-50 cursor-not-allowed"
														: ""
												}`}
											>
												{tAssetlistpartial("ChangeReservationTime")}
											</button>
											<button
												onClick={() =>
													onReturnAsset(asset.id)
												}
												disabled={loading[asset.id]}
												className={`bg-orange-500 hover:bg-orange-600 text-white font-medium py-1 px-3 rounded ${
													loading[asset.id]
														? "opacity-50 cursor-not-allowed"
														: ""
												}`}
											>
												{tAssetlistpartial("Returned")}
											</button>
											<button
												onClick={() =>
													onRemoveReservation(
														asset.id,
													)
												}
												disabled={loading[asset.id]}
												className={`bg-orange-500 hover:bg-orange-600 text-white font-medium py-1 px-3 rounded ${
													loading[asset.id]
														? "opacity-50 cursor-not-allowed"
														: ""
												}`}
											>
												{tAssetlistpartial("RemoveReservation")}
											</button>
										</>
									) : asset.reserved ? (
										<span className="text-sm text-red-500 font-medium py-1 px-3">
											{tAssetlistpartial("NotYetReturned")}
										</span>
									) : (
										<button
											onClick={() =>
												onReserveAsset(asset)
											}
											disabled={loading[asset.id]}
											className={`bg-orange-500 hover:bg-orange-600 text-white font-medium py-1 px-3 rounded ${
												loading[asset.id]
													? "opacity-50 cursor-not-allowed"
													: ""
											}`}
										>
											{tAssetlistpartial("Reserve")}
										</button>
									)}
									<button
										onClick={() => onEditAsset(asset)}
										disabled={loading[asset.id]}
										className={`bg-blue-50 hover:bg-blue-100 text-blue-600 font-medium py-1 px-3 rounded ${
											loading[asset.id]
												? "opacity-50 cursor-not-allowed"
												: ""
										}`}
									>
										{tCommon("EditLink")}
									</button>
									<button
										onClick={() => onRemoveAsset(asset)}
										disabled={loading[asset.id]}
										className={`bg-red-50 hover:bg-red-100 text-red-600 font-medium py-1 px-3 rounded ${
											loading[asset.id]
												? "opacity-50 cursor-not-allowed"
												: ""
										}`}
									>
										{tAssetlistpartial("Remove")}
									</button>
								</div>
							)}
						</div>
					))}
					</div>
				</div>
			);
		}
	};

	return (
		<div>
			{/* Accordion: Available Assets */}
			<div className="mb-4 border rounded text-white">
				<button
					type="button"
					className="w-full px-4 py-2 text-left font-semibold bg-[#ffa80d] hover:bg-[#f0941d]"
					onClick={() => handleToggleSection("availableAssets")}
				>
					{tAssetViewModel("AvailableAssets")}
				</button>
				<div
					id="notTakenContent"
					className={`p-4 ${!openSections.availableAssets ? "hidden" : ""}`}
				>
					{renderAssets(availableAssets, false)}
				</div>
			</div>
			{/* Accordion: Reserved Assets */}
			<div className="mb-4 border rounded text-white">
				<button
					type="button"
					className="w-full px-4 py-2 text-left font-semibold bg-[#ffa80d] hover:bg-[#f0941d]"
					onClick={() => handleToggleSection("assetsReservedByUser")}
				>
					{tAssetViewModel("AssetsReservedByUser")}
				</button>
				<div
					id="takenContent"
					className={`p-4 ${!openSections.assetsReservedByUser ? "hidden" : ""}`}
				>
					{renderAssets(assetsReservedByUser, true)}
				</div>
			</div>
		</div>
	);
}
