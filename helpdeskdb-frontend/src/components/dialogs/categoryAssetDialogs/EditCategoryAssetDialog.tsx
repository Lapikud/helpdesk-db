import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { SubmitHandler, useForm } from "react-hook-form";
import { Modal } from "../common/Modal";
import {
	ICategory,
	ICategoryAssetWithNames,
} from "@/types/domain/DomainTypes";

interface EditCategoryAssetDialogProps {
	open: boolean;
	categoryAsset: ICategoryAssetWithNames | null;
	categories: ICategory[];
	onClose: () => void;
	onConfirm: (
		data: ICategoryAssetWithNames,
	) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

type Inputs = {
	categoryId: string;
	comment: string;
};

export const EditCategoryAssetDialog = ({
	open,
	categoryAsset,
	categories,
	onClose,
	onConfirm,
	isLoading,
}: EditCategoryAssetDialogProps) => {
	const { t: tCategoryAssets } = useTranslation("categoryassets");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");

	const [errorMsg, setErrorMsg] = useState<string | null>(null);

	const {
		register,
		handleSubmit,
		reset,
		formState: { errors },
	} = useForm<Inputs>({
		defaultValues: { categoryId: "", comment: "" },
	});

	useEffect(() => {
		if (open && categoryAsset) {
			reset({
				categoryId: categoryAsset.categoryId,
				comment: categoryAsset.comment ?? "",
			});
			setErrorMsg(null);
		}
	}, [open, categoryAsset, reset]);

	const handleClose = () => {
		setErrorMsg(null);
		onClose();
	};

	const onSubmit: SubmitHandler<Inputs> = async (data) => {
		setErrorMsg(null);
		if (!categoryAsset) return;
		const result = await onConfirm({
			...categoryAsset,
			categoryId: data.categoryId,
			comment: data.comment,
		});
		if (result && result.error) {
			setErrorMsg(result.error);
		}
	};

	if (!categoryAsset) return null;

	return (
		<Modal open={open} onClose={handleClose}>
			<h2 className="text-xl font-bold mb-2 text-black">
				{tCommon("EditTitle")}
			</h2>
			<h4 className="text-lg text-gray-700 mb-4">
				{tCategoryAssets("CategoryAssetsSingular")}
			</h4>

			{errorMsg && (
				<div className="mb-4 p-3 bg-red-100 text-red-700 border border-red-400 rounded">
					{errorMsg}
				</div>
			)}

			<form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
				<div>
					<span className="block text-xs font-bold uppercase text-gray-500 mb-1">
						{tCategoryAssets("Asset")}
					</span>
					<span className="text-gray-900 font-medium">
						{categoryAsset.assetName}
					</span>
				</div>

				<div>
					<label htmlFor="categoryId" className="block text-gray-700 mb-2">
						{tCategoryAssets("Category")}
					</label>
					<select
						id="categoryId"
						className="w-full p-2 border border-gray-300 rounded"
						{...register("categoryId", {
							required: tValidation("Required", {
								field: tCategoryAssets("Category"),
							}),
						})}
					>
						<option value="">
							{tCommon("SelectA")} {tCategoryAssets("Category")}
						</option>
						{categories.map((category) => (
							<option key={category.id} value={category.id}>
								{category.categoryName}
							</option>
						))}
					</select>
					{errors.categoryId && (
						<span className="text-red-600 text-sm">
							{errors.categoryId.message}
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
						{isLoading ? tCommon("Processing") : tCommon("SaveButton")}
					</button>
				</div>
			</form>
		</Modal>
	);
};
