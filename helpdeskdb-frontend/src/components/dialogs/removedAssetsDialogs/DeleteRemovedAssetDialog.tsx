import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../common/Modal";
import { IRemovedAssetWithAssetName } from "@/types/domain/DomainTypes";

interface DeleteRemovedAssetDialogProps {
	open: boolean;
	removedAsset: IRemovedAssetWithAssetName | null;
	onClose: () => void;
	onConfirm: (removedAssetId: string) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const DeleteRemovedAssetDialog = ({
	open,
	removedAsset,
	onClose,
	onConfirm,
	isLoading,
}: DeleteRemovedAssetDialogProps) => {
	const { t: tRemovedAssets } = useTranslation("removedassets");
	const { t: tCommon } = useTranslation("common");

	const [errorMsg, setErrorMsg] = useState<string | null>(null);

	const handleClose = () => {
		setErrorMsg(null);
		onClose();
	};

	const handleSubmit = async () => {
		setErrorMsg(null);
		if (!removedAsset) return;
		const result = await onConfirm(removedAsset.id);
		if (result && result.error) {
			setErrorMsg(result.error);
		}
	};

	if (!removedAsset) return null;

	return (
		<Modal open={open} onClose={handleClose}>
			<h2 className="text-xl font-bold mb-2 text-black">
				{tCommon("DeleteTitle")}
			</h2>
			<h3 className="text-base text-gray-700 mb-4">
				{tCommon("DeleteConfirmQuestion")}
			</h3>
			<h4 className="text-lg font-semibold text-black mb-4">
				{tRemovedAssets("RemovedAssetsSingular")}
			</h4>

			{errorMsg && (
				<div className="mb-4 p-3 bg-red-100 text-red-700 border border-red-400 rounded">
					{errorMsg}
				</div>
			)}

			<div className="bg-gray-50 p-4 rounded-lg mb-6 border border-gray-100 space-y-3">
				<div>
					<span className="block text-xs font-bold uppercase text-gray-500 mb-1">
						{tRemovedAssets("Asset")}
					</span>
					<span className="text-gray-900 font-medium">
						{removedAsset.assetName}
					</span>
				</div>
				<div>
					<span className="block text-xs font-bold uppercase text-gray-500 mb-1">
						{tCommon("Comment")}
					</span>
					<span className="text-gray-900 font-medium">
						{removedAsset.comment || "-"}
					</span>
				</div>
				<div>
					<span className="block text-xs font-bold uppercase text-gray-500 mb-1">
						{tRemovedAssets("RemovedBy")}
					</span>
					<span className="text-gray-900 font-medium">
						{removedAsset.removedBy}
					</span>
				</div>
			</div>

			<div className="flex justify-end gap-3 mt-6">
				<button
					onClick={handleClose}
					disabled={isLoading}
					className="px-4 py-2 bg-gray-400 hover:bg-gray-300 text-white rounded font-medium transition-colors"
				>
					{tCommon("Cancel")}
				</button>
				<button
					onClick={handleSubmit}
					disabled={isLoading}
					className={`px-4 py-2 bg-red-600 hover:bg-red-700 font-medium text-white rounded transition-colors ${
						isLoading ? "opacity-50 cursor-not-allowed" : ""
					}`}
				>
					{isLoading ? tCommon("Processing") : tCommon("DeleteButton")}
				</button>
			</div>
		</Modal>
	);
};
