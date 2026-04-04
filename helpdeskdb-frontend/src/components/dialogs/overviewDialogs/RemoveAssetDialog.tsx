import {
	IAssetViewModel,
	IAssetViewModelRemove,
} from "@/types/domain/IAssetViewModels";
import { useTranslation } from "react-i18next";
import { useState } from "react";
import { Modal } from "../common/Modal";
import { useSearchParams } from "next/navigation";

interface RemoveAssetDialogProps {
	open: boolean;
	asset: IAssetViewModel | null;
	onClose: () => void;
	onConfirm: (
		assetId: string,
		removeAsset: IAssetViewModelRemove
	) => Promise<void>;
	isLoading: boolean;
}

export const RemoveAssetDialog = ({
	open,
	asset,
	onClose,
	onConfirm,
	isLoading,
}: RemoveAssetDialogProps) => {
	const { t: tCommon } = useTranslation("common");
	const { t: tRemove } = useTranslation("remove");
	const { t: tValidation } = useTranslation("validationerrors");
	const [removeComment, setRemoveComment] = useState("");
	const [commentError, setCommentError] = useState<string | null>(null);

	const searchParams = useSearchParams();
	const assetId = searchParams.get("removeId");

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

	const handleClose = () => {
		setRemoveComment("");
		setCommentError(null);
		onClose();
	};

	const handleSubmit = async () => {
		if (!validateComment(removeComment)) return;
		if (!asset) return;

		try {
			await onConfirm(assetId ?? "", {
				assetId: asset.id,
				comment: removeComment,
			});
			handleClose();
		} catch (error) {
			console.error("Remove failed:", error);
		}
	};

	if (!asset) return null;

	return (
		<Modal open={open} onClose={handleClose}>
			<h3 className="font-bold text-xl mb-2 text-black">
				{tRemove("RemoveConfirmQuestion")}
			</h3>
			<h3 className="font-bold text-lg mb-2 text-black">
				{asset.assetName}
			</h3>
			<div className="mb-4">
				<label className="block text-gray-700 mb-2">
					{tCommon("Comment")}
				</label>
				<textarea
					className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base text-black shadow-sm placeholder-gray-400 focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
					value={removeComment}
					onChange={(e) => setRemoveComment(e.target.value)}
					onBlur={() => validateComment(removeComment)}
					placeholder={tCommon("CommentPrompt")}
					maxLength={255}
				/>
				{commentError && (
					<span className="text-red-600 text-sm">{commentError}</span>
				)}
			</div>
			<div className="flex justify-end gap-3">
				<button
					onClick={handleClose}
					className="px-4 py-2 bg-gray-400 hover:bg-gray-300 rounded"
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
					{isLoading
						? tCommon("Processing")
						: tRemove("RemoveButton")}
				</button>
			</div>
		</Modal>
	);
};
