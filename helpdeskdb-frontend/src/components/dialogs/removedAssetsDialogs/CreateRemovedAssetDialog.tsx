import { useContext, useState } from "react";
import { useTranslation } from "react-i18next";
import { SubmitHandler, useForm } from "react-hook-form";
import { Modal } from "../common/Modal";
import { IAsset, IRemovedAssetAdd } from "@/types/domain/DomainTypes";
import { AccountContext } from "@/context/AccountContext";

interface CreateRemovedAssetDialogProps {
	open: boolean;
	assets: IAsset[];
	onClose: () => void;
	onConfirm: (data: IRemovedAssetAdd) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const CreateRemovedAssetDialog = ({
	open,
	assets,
	onClose,
	onConfirm,
	isLoading,
}: CreateRemovedAssetDialogProps) => {
	const { t: tRemovedAssets } = useTranslation("removedassets");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");

	const { accountInfo } = useContext(AccountContext);

	const [errorMsg, setErrorMsg] = useState<string | null>(null);

	const {
		register,
		handleSubmit,
		reset,
		formState: { errors },
	} = useForm<IRemovedAssetAdd>({
		defaultValues: { assetId: "", comment: "" },
	});

	const handleClose = () => {
		reset({ assetId: "", comment: "" });
		setErrorMsg(null);
		onClose();
	};

	const onSubmit: SubmitHandler<IRemovedAssetAdd> = async (data) => {
		setErrorMsg(null);
		const result = await onConfirm({
			assetId: data.assetId,
			comment: data.comment,
		});
		if (result && result.error) {
			setErrorMsg(result.error);
			return;
		}
		reset({ assetId: "", comment: "" });
	};

	return (
		<Modal open={open} onClose={handleClose}>
			<h2 className="text-xl font-bold mb-2 text-black">
				{tCommon("CreateTitle")}
			</h2>
			<h4 className="text-lg text-gray-700 mb-4">
				{tRemovedAssets("RemovedAssetsSingular")}
			</h4>

			{errorMsg && (
				<div className="mb-4 p-3 bg-red-100 text-red-700 border border-red-400 rounded">
					{errorMsg}
				</div>
			)}

			<form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
				<div>
					<label htmlFor="assetId" className="block text-gray-700 mb-2">
						{tRemovedAssets("Asset")}
					</label>
					<select
						id="assetId"
						className="w-full p-2 border border-gray-300 rounded"
						{...register("assetId", {
							required: tValidation("Required", {
								field: tRemovedAssets("Asset"),
							}),
						})}
					>
						<option value="">
							{tCommon("SelectAn")} {tRemovedAssets("Asset")}
						</option>
						{assets.map((asset) => (
							<option key={asset.id} value={asset.id}>
								{asset.assetName}
							</option>
						))}
					</select>
					{errors.assetId && (
						<span className="text-red-600 text-sm">
							{errors.assetId.message}
						</span>
					)}
				</div>

				<div className="mb-4">
					<label className="block text-gray-700 mb-2">
						{tRemovedAssets("User")}
					</label>
					<div className="w-full p-2 border border-gray-200 rounded bg-gray-50 text-gray-700">
						{accountInfo?.name ?? ""}
					</div>
				</div>

				<div>
					<label htmlFor="comment" className="block text-gray-700 mb-2">
						{tCommon("Comment")}
					</label>
					<input
						id="comment"
						type="text"
						className="w-full p-2 border border-gray-300 rounded"
						placeholder={tCommon("CommentPrompt")}
						{...register("comment", {
							required: {
								value: true,
								message: tValidation("Required", {
									field: tCommon("Comment"),
								}),
							},
							minLength: {
								value: 2,
								message: tValidation("MinLenghtValidationError", {
									field: tCommon("Comment"),
									min: 2,
								}),
							},
							maxLength: {
								value: 255,
								message: tValidation("MaxLengthValidationError", {
									field: tCommon("Comment"),
									max: 255,
								}),
							},
						})}
					/>
					{errors.comment && (
						<span className="text-red-600 text-sm">
							{errors.comment.message}
						</span>
					)}
				</div>

				<div className="flex justify-end gap-3 mt-6">
					<button
						type="button"
						onClick={handleClose}
						disabled={isLoading}
						className="px-4 py-2 bg-gray-400 hover:bg-gray-300 text-white rounded font-medium transition-colors"
					>
						{tCommon("Cancel")}
					</button>
					<button
						type="submit"
						disabled={isLoading}
						className={`px-4 py-2 bg-orange-500 hover:bg-orange-600 font-medium text-white rounded transition-colors ${
							isLoading ? "opacity-50 cursor-not-allowed" : ""
						}`}
					>
						{isLoading ? tCommon("Processing") : tCommon("CreateButton")}
					</button>
				</div>
			</form>
		</Modal>
	);
};
