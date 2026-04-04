import { IAssetViewModel, IAssetViewModelUpdate } from "@/types/domain/IAssetViewModels";
import { useTranslation } from "react-i18next";
import { useEffect, useState } from "react";
import { Modal } from "../common/Modal";
import { ICategory, ILocation, IOwner, ICategoryAsset, ILocationAsset, IOwnerAsset } from "@/types/domain/DomainTypes";
import { useSearchParams } from "next/navigation";

interface IEditAssetDialogProps {
	open: boolean;
	asset: IAssetViewModel | null;
	comment: string;
	categoryAssets: ICategoryAsset | null;
	ownerAssets: IOwnerAsset | null;
	locationAssets: ILocationAsset | null;
	categories: ICategory[];
	locations: ILocation[];
	owners: IOwner[];
	onClose: () => void;
	onConfirm: (
		assetId: string,
		updateAsset: IAssetViewModelUpdate
	) => Promise<void>;
	isLoading: boolean;
}

export const EditAssetDialog = ({
	open,
	asset,
	comment,
	categoryAssets,
	ownerAssets,
	locationAssets,
	categories,
	locations,
	owners,
	onClose,
	onConfirm,
	isLoading,
}: IEditAssetDialogProps) => {
	const { t: tCommon } = useTranslation("common");
	const { t: tAsset } = useTranslation("asset");
	const { t: tAssetViewModel } = useTranslation("assetviewmodel");
	const { t: tValidation } = useTranslation("validationerrors");

	const searchParams = useSearchParams();
	const assetId = searchParams.get("editId");

	const [updateComment, setUpdateComment] = useState("");
	const [updateAssetName, setUpdateAssetName] = useState("");
	const [updateSerialNumber, setUpdateSerialNumber] = useState("");
	const [updateBarcode, setUpdateBarcode] = useState("");
	const [categoryId, setCategoryId] = useState(""); // new selected category
	const [ownerId, setOwnerId] = useState(""); // new selected owner
	const [locationId, setLocationId] = useState(""); // new selected location
	const [commentError, setCommentError] = useState<string | null>(null);
	const [assetNameError, setAssetNameError] = useState<string | null>(null);
	const [serialNumberError, setSerialNumberError] = useState<string | null>(null);
	const [barcodeError, setBarcodeError] = useState<string | null>(null);
	const [categoryError, setCategoryError] = useState<string | null>(null);
	const [ownerError, setOwnerError] = useState<string | null>(null);
	const [locationError, setLocationError] = useState<string | null>(null);

	useEffect(() => {
		if (asset) {
			setUpdateAssetName(asset.assetName);
			setUpdateComment(comment);
			setUpdateSerialNumber(asset.serialNumber ?? "");
			setUpdateBarcode(asset.barcode ?? "");
			setCategoryId(categoryAssets?.categoryId ?? "");
			setOwnerId(ownerAssets?.ownerId ?? "");
			setLocationId(locationAssets?.locationId ?? "");
		}
	}, [asset, comment, open, categoryAssets, locationAssets, ownerAssets]);

	const validateComment = (comment: string) => {
		if (!comment || comment.trim().length === 0) {
			setCommentError(
				tValidation("Required", { field: tCommon("Comment") })
			);
			return false;
		}
		if (comment.length < 2) {
			setCommentError(
				tValidation("MinLenghtValidationError", {
					field: tCommon("Comment"),
					min: 2,
				})
			);
			return false;
		}
		if (comment.length > 255) {
			setCommentError(
				tValidation("MaxLengthValidationError", {
					field: tCommon("Comment"),
					max: 255,
				})
			);
			return false;
		}
		setCommentError(null);
		return true;
	};

	const validateAssetName = (assetName: string) => {
		if (!assetName || assetName.trim().length === 0) {
			setAssetNameError(
				tValidation("Required", { field: tAsset("AssetName") })
			);
			return false;
		}
		if (assetName.length < 2) {
			setAssetNameError(
				tValidation("MinLenghtValidationError", {
					field: tAsset("AssetName"),
					min: 2,
				})
			);
			return false;
		}
		if (assetName.length > 128) {
			setAssetNameError(
				tValidation("MaxLengthValidationError", {
					field: tAsset("AssetName"),
					max: 128,
				})
			);
			return false;
		}
		setAssetNameError(null);
		return true;
	};

	const validateSerialNumber = (serialNumber: string) => {
		if (serialNumber.length > 255) {
			setSerialNumberError(
				tValidation("MaxLengthValidationError", {
					field: tAsset("SerialNumber"),
					max: 255,
				})
			);
			return false;
		}
		setSerialNumberError(null);
		return true;
	};

	const validateBarcode = (barcode: string) => {
		if (barcode.length > 255) {
			setBarcodeError(
				tValidation("MaxLengthValidationError", {
					field: tAsset("Barcode"),
					max: 255,
				})
			);
			return false;
		}
		setBarcodeError(null);
		return true;
	};

	const validateCategory = (categoryId: string) => {
		if (!categoryId || categoryId.trim().length === 0) {
			setCategoryError(
				tValidation("Required", { field: tAssetViewModel("Category") })
			);
			return false;
		}
		setCategoryError(null);
		return true;
	};

	const validateOwner = (ownerId: string) => {
		if (!ownerId || ownerId.trim().length === 0) {
			setOwnerError(
				tValidation("Required", { field: tAssetViewModel("Owner") })
			);
			return false;
		}
		setOwnerError(null);
		return true;
	};

	const validateLocation = (locationId: string) => {
		if (!locationId || locationId.trim().length === 0) {
			setLocationError(
				tValidation("Required", { field: tAssetViewModel("Location") })
			);
			return false;
		}
		setLocationError(null);
		return true;
	};

	const handleClose = () => {
		setUpdateComment("");
		setUpdateAssetName("");
		setUpdateSerialNumber("");
		setUpdateBarcode("");
		setCategoryId("");
		setOwnerId("");
		setLocationId("");
		setCommentError(null);
		setAssetNameError(null);
		setSerialNumberError(null);
		setBarcodeError(null);
		setCategoryError(null);
		setOwnerError(null);
		setLocationError(null);
		onClose();
	};

	const handleSubmit = async () => {
		if (!validateAssetName(updateAssetName)) return;
		if (!validateComment(updateComment)) return;
		if (!validateSerialNumber(updateSerialNumber)) return;
		if (!validateBarcode(updateBarcode)) return;
		if (!validateCategory(categoryId)) return;
		if (!validateOwner(ownerId)) return;
		if (!validateLocation(locationId)) return;
		if (!asset) return;

		try {
			await onConfirm(assetId ?? "", {
				assetId: asset.id,
				assetName: updateAssetName,
				comment: updateComment,
				serialNumber: updateSerialNumber || null,
				barcode: updateBarcode || null,
				selectedCategoryId: categoryId,
				selectedOwnerId: ownerId,
				selectedLocationId: locationId,
				categoryAssetsId: categoryAssets?.id ?? null,
				ownerAssetsId: ownerAssets?.id ?? null,
				locationAssetsId: locationAssets?.id ?? null,
			});
			handleClose();
		} catch (error) {
			console.error("Edit failed:", error);
		}
	};
	if (!asset) {
		return null;
	}

	return (
		<Modal open={open} onClose={handleClose}>
			<h3 className="font-bold text-xl mb-2">{tCommon("EditTitle")}</h3>
			<div className="mb-4">
				<label className="block text-gray-700 mb-2">
					{tAsset("AssetName")}
				</label>
				<input
					className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base text-black shadow-sm placeholder-gray-400 focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
					value={updateAssetName}
					onChange={(e) => setUpdateAssetName(e.target.value)}
					onBlur={() => validateAssetName(updateAssetName)}
					placeholder={tAsset("AssetNamePrompt")}
				/>
				{assetNameError && (
					<span className="text-red-600 text-sm">
						{assetNameError}
					</span>
				)}
			</div>
			<div className="mb-4">
				<label className="block text-gray-700 mb-2">
					{tCommon("Comment")}
				</label>
				<input
					className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base text-black shadow-sm placeholder-gray-400 focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
					value={updateComment}
					onChange={(e) => setUpdateComment(e.target.value)}
					onBlur={() => validateComment(updateComment)}
					placeholder={tCommon("CommentPrompt")}
					maxLength={255}
				/>
				{commentError && (
					<span className="text-red-600 text-sm">{commentError}</span>
				)}
			</div>
			<div className="mb-4">
				<label className="block text-gray-700 mb-2">
					{tAsset("SerialNumber")}
				</label>
				<input
					className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base text-black shadow-sm placeholder-gray-400 focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
					value={updateSerialNumber}
					onChange={(e) => { setUpdateSerialNumber(e.target.value); setSerialNumberError(null); }}
					onBlur={() => validateSerialNumber(updateSerialNumber)}
					placeholder={tAsset("SerialNumberPrompt")}
					maxLength={255}
				/>
				{serialNumberError && (
					<span className="text-red-600 text-sm">{serialNumberError}</span>
				)}
			</div>
			<div className="mb-4">
				<label className="block text-gray-700 mb-2">
					{tAsset("Barcode")}
				</label>
				<input
					className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base text-black shadow-sm placeholder-gray-400 focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
					value={updateBarcode}
					onChange={(e) => { setUpdateBarcode(e.target.value); setBarcodeError(null); }}
					onBlur={() => validateBarcode(updateBarcode)}
					placeholder={tAsset("BarcodePrompt")}
					maxLength={255}
				/>
				{barcodeError && (
					<span className="text-red-600 text-sm">{barcodeError}</span>
				)}
			</div>
			<div className="relative mb-4">
				<label
					className="block mb-1 text-sm font-medium text-gray-700"
					htmlFor="SelectedCategoryId"
				>
					{tAssetViewModel("Category")}
				</label>
				<select
					className="w-full p-2 border border-gray-300 rounded"
					value={categoryId}
					onChange={(e) => setCategoryId(e.target.value)}
					onBlur={() => validateCategory(categoryId)}
				>
					<option value="">
						{tCommon("SelectA")} {tAssetViewModel("Category")}
					</option>
					{categories.map((category) => (
						<option key={category.id} value={category.id}>
							{category.categoryName}
						</option>
					))}
				</select>
				{categoryError && (
					<span className="text-red-600 text-sm">
						{categoryError}
					</span>
				)}
			</div>
			<div className="mb-4">
				<label className="block text-gray-700 mb-2">
					{tAssetViewModel("Owner")}
				</label>
				<select
					className="w-full p-2 border border-gray-300 rounded"
					value={ownerId}
					onChange={(e) => setOwnerId(e.target.value)}
					onBlur={() => validateOwner(ownerId)}
				>
					<option value="">
						{tCommon("SelectAn")} {tAssetViewModel("Owner")}
					</option>
					{owners.map((owner) => (
						<option key={owner.id} value={owner.id}>
							{owner.ownerName}
						</option>
					))}
				</select>
				{ownerError && (
					<span className="text-red-600 text-sm">{ownerError}</span>
				)}
			</div>
			<div className="mb-4">
				<label className="block text-gray-700 mb-2">
					{tAssetViewModel("Location")}
				</label>
				<select
					className="w-full p-2 border border-gray-300 rounded"
					value={locationId}
					onChange={(e) => setLocationId(e.target.value)}
					onBlur={() => validateLocation(locationId)}
				>
					<option value="">
						{tCommon("SelectA")} {tAssetViewModel("Location")}
					</option>
					{locations.map((location) => (
						<option key={location.id} value={location.id}>
							{location.locationName}
						</option>
					))}
				</select>
				{locationError && (
					<span className="text-red-600 text-sm">
						{locationError}
					</span>
				)}
			</div>
			<div className="flex justify-end gap-3">
				<button
					onClick={handleClose}
					className="px-4 py-2 bg-gray-200 hover:bg-gray-300 rounded"
					disabled={isLoading}
				>
					{tCommon("Cancel")}
				</button>
				<button
					onClick={handleSubmit}
					disabled={isLoading}
					className={`px-4 py-2 bg-red-500 hover:bg-red-600 text-white rounded ${
						isLoading ? "opacity-50 cursor-not-allowed" : ""
					}`}
				>
					{tCommon("SaveButton")}
				</button>
			</div>
		</Modal>
	);
};
