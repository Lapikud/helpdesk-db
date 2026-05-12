import { useState } from "react";
import { useTranslation } from "react-i18next";
import { SubmitHandler, useForm } from "react-hook-form";
import { Modal } from "../common/Modal";
import { IAssetAdd } from "@/types/domain/DomainTypes";

interface CreateDbAssetDialogProps {
	open: boolean;
	onClose: () => void;
	onConfirm: (data: IAssetAdd) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

type Inputs = {
	assetName: string;
	comment: string;
	serialNumber: string;
	barcode: string;
};

export const CreateDbAssetDialog = ({
	open,
	onClose,
	onConfirm,
	isLoading,
}: CreateDbAssetDialogProps) => {
	const { t: tAsset } = useTranslation("asset");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");

	const [errorMsg, setErrorMsg] = useState<string | null>(null);

	const {
		register,
		handleSubmit,
		reset,
		formState: { errors },
	} = useForm<Inputs>({
		defaultValues: { assetName: "", comment: "", serialNumber: "", barcode: "" },
	});

	const handleClose = () => {
		reset({ assetName: "", comment: "", serialNumber: "", barcode: "" });
		setErrorMsg(null);
		onClose();
	};

	const onSubmit: SubmitHandler<Inputs> = async (data) => {
		setErrorMsg(null);
		const result = await onConfirm({
			assetName: data.assetName,
			comment: data.comment,
			serialNumber: data.serialNumber || null,
			barcode: data.barcode || null,
		});
		if (result && result.error) {
			setErrorMsg(result.error);
			return;
		}
		reset({ assetName: "", comment: "", serialNumber: "", barcode: "" });
	};

	return (
		<Modal open={open} onClose={handleClose}>
			<h2 className="text-xl font-bold mb-2 text-black">
				{tCommon("CreateTitle")}
			</h2>
			<h4 className="text-lg text-gray-700 mb-4">
				{tAsset("AssetSingular")}
			</h4>

			{errorMsg && (
				<div className="mb-4 p-3 bg-red-100 text-red-700 border border-red-400 rounded">
					{errorMsg}
				</div>
			)}

			<form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
				<div>
					<label htmlFor="assetName" className="block text-gray-700 mb-2">
						{tAsset("AssetName")}
					</label>
					<input
						id="assetName"
						type="text"
						className="w-full p-2 border border-gray-300 rounded"
						placeholder={tAsset("AssetNamePrompt")}
						{...register("assetName", {
							required: {
								value: true,
								message: tValidation("Required", {
									field: tAsset("AssetName"),
								}),
							},
							minLength: {
								value: 2,
								message: tValidation("MinLenghtValidationError", {
									field: tAsset("AssetName"),
									min: 2,
								}),
							},
							maxLength: {
								value: 128,
								message: tValidation("MaxLengthValidationError", {
									field: tAsset("AssetName"),
									max: 128,
								}),
							},
						})}
					/>
					{errors.assetName && (
						<span className="text-red-600 text-sm">
							{errors.assetName.message}
						</span>
					)}
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

				<div>
					<label htmlFor="serialNumber" className="block text-gray-700 mb-2">
						{tAsset("SerialNumber")}
					</label>
					<input
						id="serialNumber"
						type="text"
						className="w-full p-2 border border-gray-300 rounded"
						placeholder={tAsset("SerialNumberPrompt")}
						{...register("serialNumber", {
							maxLength: {
								value: 255,
								message: tValidation("MaxLengthValidationError", {
									field: tAsset("SerialNumber"),
									max: 255,
								}),
							},
						})}
					/>
					{errors.serialNumber && (
						<span className="text-red-600 text-sm">
							{errors.serialNumber.message}
						</span>
					)}
				</div>

				<div>
					<label htmlFor="barcode" className="block text-gray-700 mb-2">
						{tAsset("Barcode")}
					</label>
					<input
						id="barcode"
						type="text"
						className="w-full p-2 border border-gray-300 rounded"
						placeholder={tAsset("BarcodePrompt")}
						{...register("barcode", {
							maxLength: {
								value: 255,
								message: tValidation("MaxLengthValidationError", {
									field: tAsset("Barcode"),
									max: 255,
								}),
							},
						})}
					/>
					{errors.barcode && (
						<span className="text-red-600 text-sm">
							{errors.barcode.message}
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
