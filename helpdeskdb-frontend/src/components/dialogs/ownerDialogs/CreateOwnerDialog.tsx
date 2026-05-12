import { useState } from "react";
import { useTranslation } from "react-i18next";
import { SubmitHandler, useForm } from "react-hook-form";
import { Modal } from "../common/Modal";
import { IOwnerAdd } from "@/types/domain/DomainTypes";

interface CreateOwnerDialogProps {
	open: boolean;
	onClose: () => void;
	onConfirm: (data: IOwnerAdd) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const CreateOwnerDialog = ({
	open,
	onClose,
	onConfirm,
	isLoading,
}: CreateOwnerDialogProps) => {
	const { t: tOwner } = useTranslation("owner");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");

	const [errorMsg, setErrorMsg] = useState<string | null>(null);

	const {
		register,
		handleSubmit,
		reset,
		formState: { errors },
	} = useForm<IOwnerAdd>({
		defaultValues: { ownerName: "", comment: "" },
	});

	const handleClose = () => {
		reset({ ownerName: "", comment: "" });
		setErrorMsg(null);
		onClose();
	};

	const onSubmit: SubmitHandler<IOwnerAdd> = async (data) => {
		setErrorMsg(null);
		const result = await onConfirm({
			ownerName: data.ownerName,
			comment: data.comment,
		});
		if (result && result.error) {
			setErrorMsg(result.error);
			return;
		}
		reset({ ownerName: "", comment: "" });
	};

	return (
		<Modal open={open} onClose={handleClose}>
			<h2 className="text-xl font-bold mb-2 text-black">
				{tCommon("CreateTitle")}
			</h2>
			<h4 className="text-lg text-gray-700 mb-4">
				{tOwner("OwnerSingular")}
			</h4>

			{errorMsg && (
				<div className="mb-4 p-3 bg-red-100 text-red-700 border border-red-400 rounded">
					{errorMsg}
				</div>
			)}

			<form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
				<div>
					<label htmlFor="ownerName" className="block text-gray-700 mb-2">
						{tOwner("OwnerName")}
					</label>
					<input
						id="ownerName"
						type="text"
						className="w-full p-2 border border-gray-300 rounded"
						placeholder={tOwner("OwnerNamePrompt")}
						{...register("ownerName", {
							required: {
								value: true,
								message: tValidation("Required", {
									field: tOwner("OwnerName"),
								}),
							},
							minLength: {
								value: 2,
								message: tValidation("MinLenghtValidationError", {
									field: tOwner("OwnerName"),
									min: 2,
								}),
							},
							maxLength: {
								value: 128,
								message: tValidation("MaxLengthValidationError", {
									field: tOwner("OwnerName"),
									max: 128,
								}),
							},
						})}
					/>
					{errors.ownerName && (
						<span className="text-red-600 text-sm">
							{errors.ownerName.message}
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
						{isLoading ? tCommon("Processing") : tCommon("CreateButton")}
					</button>
				</div>
			</form>
		</Modal>
	);
};
