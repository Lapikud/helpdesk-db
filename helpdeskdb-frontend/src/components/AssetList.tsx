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
	onModeChange: (mode: string) => void;
	searchInput: string;
	onSearchChange: (value: string) => void;
	onSearchSubmit: (e: React.FormEvent) => void;
	createLoading: boolean;
	onCreateAsset: () => void;
}

function ChevronIcon({ open }: { open: boolean }) {
	return (
		<svg
			className={`w-5 h-5 text-[#424242] transition-transform duration-200 ${open ? "" : "rotate-180"}`}
			fill="none"
			viewBox="0 0 24 24"
			stroke="currentColor"
		>
			<path
				strokeLinecap="round"
				strokeLinejoin="round"
				strokeWidth={2}
				d="M19 9l-7 7-7-7"
			/>
		</svg>
	);
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
	onModeChange,
	searchInput,
	onSearchChange,
	onSearchSubmit,
	createLoading,
	onCreateAsset,
}: AssetListProps) {
	const { t: tAssetlistpartial } = useTranslation("_assetlistpartial");
	const { t: tCommon } = useTranslation("common");
	const { t: tAssetViewModel } = useTranslation("assetviewmodel");

	const { accountInfo } = useContext(AccountContext);

	const isAdmin = accountInfo?.roles?.includes("admins") ?? false;
	const isMember = accountInfo?.roles?.includes("members") ?? false;
	const isPixel = accountInfo?.roles?.includes("pixels") ?? false;
	const showActions = isAdmin || isMember || isPixel;

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

	const isExpiredNotReturned = (asset: IAssetViewModel, isReserved: boolean) =>
		isReserved &&
		asset.reservationTo != null &&
		new Date(asset.reservationTo) < new Date();

	const renderActionButtons = (asset: IAssetViewModel, isReserved: boolean) => {
		if (!showActions) return null;
		const isDisabled = loading[asset.id];
		const btnBase =
			"text-sm font-medium py-2 px-4 rounded-xl whitespace-nowrap transition-colors duration-150 disabled:opacity-50 disabled:cursor-not-allowed";

		return (
			<div className="flex flex-col gap-1.5 items-stretch">
				{isReserved ? (
					<>
						{isExpiredNotReturned(asset, true) ? (
							<button
								onClick={() => onReturnAsset(asset.id)}
								disabled={isDisabled}
								className={`${btnBase} bg-[#ff9800] hover:bg-[#f0941d] text-white`}
							>
								{tAssetlistpartial("Returned")}
							</button>
						) : (
							<>
								<button
									onClick={() => onReturnAsset(asset.id)}
									disabled={isDisabled}
									className={`${btnBase} bg-[#ff9800] hover:bg-[#f0941d] text-white`}
								>
									{tAssetlistpartial("Returned")}
								</button>
								<button
									onClick={() =>
										onChangeReservationTime(
											asset.reservationId!,
										)
									}
									disabled={isDisabled || !asset.reservationId}
									className={`${btnBase} bg-[#ff9800] hover:bg-[#f0941d] text-white`}
								>
									{tAssetlistpartial("ChangeReservationTime")}
								</button>
								<button
									onClick={() => onRemoveReservation(asset.id)}
									disabled={isDisabled}
									className={`${btnBase} bg-[#ff9800] hover:bg-[#f0941d] text-white`}
								>
									{tAssetlistpartial("RemoveReservation")}
								</button>
							</>
						)}
					</>
				) : asset.reserved ? (
					<span className="text-sm text-red-500 font-medium py-2 px-4 text-center">
						{tAssetlistpartial("NotYetReturned")}
					</span>
				) : (
					<button
						onClick={() => onReserveAsset(asset)}
						disabled={isDisabled}
						className={`${btnBase} bg-[#ff9800] hover:bg-[#f0941d] text-white`}
					>
						{tAssetlistpartial("Reserve")}
					</button>
				)}
				<button
					onClick={() => onEditAsset(asset)}
					disabled={isDisabled}
					className={`${btnBase} bg-[#e3f2fd] hover:bg-blue-100 text-[#50b3f1]`}
				>
					{tCommon("EditLink")}
				</button>
				<button
					onClick={() => onRemoveAsset(asset)}
					disabled={isDisabled}
					className={`${btnBase} bg-[#ffebee] hover:bg-red-100 text-[#ea6e6c]`}
				>
					{tAssetlistpartial("Remove")}
				</button>
			</div>
		);
	};

	const renderTableHeader = () => (
		<div
			className={`bg-[#424242] text-white text-sm rounded-full mx-4 mt-4 px-6 py-3 grid gap-1 ${showActions ? "grid-cols-11" : "grid-cols-10"}`}
		>
			<div className="text-center">{tAssetViewModel("AssetName")}</div>
			<div className="text-center">
				<span className="block leading-tight">
					{tAssetViewModel("SerialNumber")}
				</span>
			</div>
			<div className="text-center">{tAssetViewModel("Category")}</div>
			<div className="text-center">{tAssetViewModel("Owner")}</div>
			<div className="text-center">{tAssetViewModel("RoomName")}</div>
			<div className="text-center">{tAssetViewModel("CupboardName")}</div>
			<div className="text-center">
				<span className="block leading-tight">
					{tAssetViewModel("ShelfNum")}
				</span>
			</div>
			<div className="text-center">{tAssetViewModel("Column")}</div>
			<div className="text-center">
				<span className="block leading-tight">
					{tAssetViewModel("ClosestReservationBy")}
				</span>
			</div>
			<div className="text-center">{tCommon("AddedAt")}</div>
			{showActions && (
				<div className="text-center">{tAssetlistpartial("Actions")}</div>
			)}
		</div>
	);

	const renderLines = (assets: IAssetViewModel[], isReserved: boolean) => {
		if (assets.length === 0) {
			return (
				<div className="text-center py-8 text-gray-500 px-4 pb-4">
					{tAssetlistpartial("NoAssetsToDisplay")}
				</div>
			);
		}
		return (
			<div className="mx-4 mb-4 overflow-x-auto">
				<div className="min-w-[860px] bg-[#efefef] rounded-2xl overflow-hidden">
				{renderTableHeader()}
				<div className="flex flex-col gap-1 p-4 max-h-[520px] overflow-y-auto">
					{assets.map((asset) => (
						<div
							key={asset.id}
							className={`bg-white rounded-2xl px-6 py-3 grid gap-1 items-center text-sm text-black ${
								showActions ? "grid-cols-11" : "grid-cols-10"
							} ${isExpiredNotReturned(asset, isReserved) ? "!bg-red-50" : ""}`}
						>
							<AssetLineDetails asset={asset} />
							{showActions && (
								<div className="flex justify-center">
									{renderActionButtons(asset, isReserved)}
								</div>
							)}
						</div>
					))}
				</div>
				</div>
			</div>
		);
	};

	const renderCards = (assets: IAssetViewModel[], isReserved: boolean) => {
		if (assets.length === 0) {
			return (
				<div className="text-center py-8 text-gray-500 px-4 pb-4">
					{tAssetlistpartial("NoAssetsToDisplay")}
				</div>
			);
		}
		return (
			<div className="overflow-y-auto max-h-[520px] px-4 pb-4">
				<ul className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
					{assets.map((asset) => (
						<li
							key={asset.id}
							className={`rounded-2xl shadow-sm overflow-hidden border hover:shadow-md transition-shadow duration-200 ${
								isExpiredNotReturned(asset, isReserved)
									? "bg-red-50 border-red-200"
									: "bg-white border-gray-100"
							}`}
						>
							<div className="p-4">
								<AssetCardDetails asset={asset} />
								{showActions && (
									<div className="mt-4 pt-4 border-t border-gray-100 flex flex-col gap-2">
										{renderActionButtons(asset, isReserved)}
									</div>
								)}
							</div>
						</li>
					))}
				</ul>
			</div>
		);
	};

	const renderAssets = (assets: IAssetViewModel[], isReserved: boolean) =>
		mode === "cards"
			? renderCards(assets, isReserved)
			: renderLines(assets, isReserved);

	return (
		<div className="text-left">
			{/* Available assets section */}
			<div className="mb-6 bg-white rounded-2xl shadow-sm overflow-hidden">
				<div className="flex items-center flex-wrap gap-3 justify-between px-6 py-5">
					<h2 className="text-xl font-semibold text-[#424242] whitespace-nowrap">
						{tAssetViewModel("AvailableAssets")}
					</h2>
					<div className="flex items-center gap-3 flex-wrap justify-end">
						{/* Mode toggle */}
						<div className="flex items-center gap-4">
							<button
								type="button"
								onClick={() => onModeChange("lines")}
								className="flex items-center gap-1.5"
							>
								<span
									className={`w-[22px] h-[22px] rounded-full border-2 flex items-center justify-center transition-colors ${
										mode === "lines"
											? "border-[#ff9800]"
											: "border-gray-300"
									}`}
								>
									{mode === "lines" && (
										<span className="w-3 h-3 rounded-full bg-[#ff9800]" />
									)}
								</span>
								<span className="text-sm text-[#424242]">
									{tAssetlistpartial("ShowLines")}
								</span>
							</button>
							<button
								type="button"
								onClick={() => onModeChange("cards")}
								className="flex items-center gap-1.5"
							>
								<span
									className={`w-[22px] h-[22px] rounded-full border-2 flex items-center justify-center transition-colors ${
										mode === "cards"
											? "border-[#ff9800]"
											: "border-gray-300"
									}`}
								>
									{mode === "cards" && (
										<span className="w-3 h-3 rounded-full bg-[#ff9800]" />
									)}
								</span>
								<span className="text-sm text-[#424242]">
									{tAssetlistpartial("ShowCards")}
								</span>
							</button>
						</div>
						{/* Search */}
						<form
							onSubmit={onSearchSubmit}
							className="relative flex items-center"
						>
							<input
								type="text"
								className="bg-[#efefef] rounded-full pl-4 pr-10 py-2 text-sm text-[#585858] placeholder-[#585858] focus:outline-none w-52"
								placeholder={tCommon("Search") + "..."}
								value={searchInput}
								onChange={(e) => onSearchChange(e.target.value)}
							/>
							<button
								type="submit"
								className="absolute right-3 text-gray-500 hover:text-gray-700"
							>
								<svg
									xmlns="http://www.w3.org/2000/svg"
									className="h-4 w-4"
									fill="none"
									viewBox="0 0 24 24"
									stroke="currentColor"
								>
									<path
										strokeLinecap="round"
										strokeLinejoin="round"
										strokeWidth={2}
										d="M21 21l-4.35-4.35m0 0A7.5 7.5 0 104.5 4.5a7.5 7.5 0 0012.15 12.15z"
									/>
								</svg>
							</button>
						</form>
						{/* Create button */}
						{showActions && (
							<button
								type="button"
								onClick={onCreateAsset}
								disabled={createLoading}
								className={`bg-[#ff9800] hover:bg-[#f0941d] text-white font-medium px-5 py-2 rounded-full text-sm whitespace-nowrap transition-colors duration-150 ${
									createLoading
										? "opacity-50 cursor-not-allowed"
										: ""
								}`}
							>
								{tAssetViewModel("CreateNewAsset")}
							</button>
						)}
						{/* Collapse toggle */}
						<button
							type="button"
							onClick={() => handleToggleSection("availableAssets")}
							className="p-1 flex-shrink-0"
							aria-label="Toggle section"
						>
							<ChevronIcon open={openSections.availableAssets} />
						</button>
					</div>
				</div>
				{openSections.availableAssets &&
					renderAssets(availableAssets, false)}
			</div>

			{/* Users reservations section */}
			<div className="mb-6 bg-white rounded-2xl shadow-sm overflow-hidden">
				<div className="flex items-center justify-between px-6 py-5">
					<h2 className="text-xl font-semibold text-[#424242]">
						{tAssetViewModel("AssetsReservedByUser")}
					</h2>
					<button
						type="button"
						onClick={() =>
							handleToggleSection("assetsReservedByUser")
						}
						className="p-1"
						aria-label="Toggle section"
					>
						<ChevronIcon open={openSections.assetsReservedByUser} />
					</button>
				</div>
				{openSections.assetsReservedByUser &&
					renderAssets(assetsReservedByUser, true)}
			</div>
		</div>
	);
}
