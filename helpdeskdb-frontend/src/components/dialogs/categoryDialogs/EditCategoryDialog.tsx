import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { SubmitHandler, useForm } from "react-hook-form";
import { Modal } from "../common/Modal";
import { ICategory } from "@/types/domain/DomainTypes";

interface EditCategoryDialogProps {
	open: boolean;
	category: ICategory | null;
	onClose: () => void;
	onConfirm: (data: ICategory) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

type Inputs = {
	categoryName: string;
	comment: string;
};

export const EditCategoryDialog = ({
	open,
	category,
	onClose,
	onConfirm,
	isLoading,
}: EditCategoryDialogProps) => {
	const { t: tCategory } = useTranslation("category");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");

	const [errorMsg, setErrorMsg] = useState<string | null>(null);

	const {
		register,
		handleSubmit,
		reset,
		formState: { errors },
	} = useForm<Inputs>({
		defaultValues: { categoryName: "", comment: "" },
	});

	useEffect(() => {
		if (open && category) {
			reset({
				categoryName: category.categoryName,
				comment: category.comment,
			});
			setErrorMsg(null);
		}
	}, [open, category, reset]);

	const handleClose = () => {
		setErrorMsg(null);
		onClose();
	};

	const onSubmit: SubmitHandler<Inputs> = async (data) => {
		setErrorMsg(null);
		if (!category) return;
		const result = await onConfirm({
			id: category.id,
			categoryName: data.categoryName,
			comment: data.comment,
		});
		if (result && result.error) {
			setErrorMsg(result.error);
		}
	};

	if (!category) return null;

	return (
		<Modal open={open} onClose={handleClose}>
			<h2 className="text-xl font-bold mb-2 text-black">
				{tCommon("EditTitle")}
			</h2>
			<h4 className="text-lg text-gray-700 mb-4">
				{tCategory("CategorySingular")}
			</h4>

			{errorMsg && (
				<div className="mb-4 p-3 bg-red-100 text-red-700 border border-red-400 rounded">
					{errorMsg}
				</div>
			)}

			<form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
				<div>
					<label
						htmlFor="categoryName"
						className="block text-gray-700 mb-2"
					>
						{tCategory("CategoryName")}
					</label>
					<input
						id="categoryName"
						type="text"
						className="w-full p-2 border border-gray-300 rounded"
						placeholder={tCategory("CategoryNamePrompt")}
						{...register("categoryName", {
							required: {
								value: true,
								message: tValidation("Required", {
									field: tCategory("CategoryName"),
								}),
							},
							minLength: {
								value: 2,
								message: tValidation("MinLenghtValidationError", {
									field: tCategory("CategoryName"),
									min: 2,
								}),
							},
							maxLength: {
								value: 128,
								message: tValidation("MaxLengthValidationError", {
									field: tCategory("CategoryName"),
									max: 128,
								}),
							},
						})}
					/>
					{errors.categoryName && (
						<span className="text-red-600 text-sm">
							{errors.categoryName.message}
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
