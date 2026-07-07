import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "./Modal";
import { ConfirmResult, DeleteSummaryField } from "./entityDialogTypes";

interface EntityDeleteDialogProps<TEntity extends { id: string }> {
	open: boolean;
	entity: TEntity | null;
	namespace: string;
	singularKey: string;
	summaryFields: DeleteSummaryField<TEntity>[];
	onClose: () => void;
	onConfirm: (id: string) => ConfirmResult;
	isLoading: boolean;
}

export const EntityDeleteDialog = <TEntity extends { id: string }>({
	open,
	entity,
	namespace,
	singularKey,
	summaryFields,
	onClose,
	onConfirm,
	isLoading,
}: EntityDeleteDialogProps<TEntity>) => {
	const { t: tEntity } = useTranslation(namespace);
	const { t: tCommon } = useTranslation("common");

	const [errorMsg, setErrorMsg] = useState<string | null>(null);

	const handleClose = () => {
		setErrorMsg(null);
		onClose();
	};

	const handleSubmit = async () => {
		setErrorMsg(null);
		if (!entity) return;
		const result = await onConfirm(entity.id);
		if (result && result.error) {
			setErrorMsg(result.error);
		}
	};

	if (!entity) return null;

	return (
		<Modal open={open} onClose={handleClose}>
			<h2 className="text-xl font-bold mb-2 text-black">
				{tCommon("DeleteTitle")}
			</h2>
			<h3 className="text-base text-gray-700 mb-4">
				{tCommon("DeleteConfirmQuestion")}
			</h3>
			<h4 className="text-lg font-semibold text-black mb-4">
				{tEntity(singularKey)}
			</h4>

			{errorMsg && (
				<div className="mb-4 p-3 bg-red-100 text-red-700 border border-red-400 rounded">
					{errorMsg}
				</div>
			)}

			<div className="bg-gray-50 p-4 rounded-lg mb-6 border border-gray-100 space-y-3">
				{summaryFields.map((field) => (
					<div key={field.labelKey}>
						<span className="block text-xs font-bold uppercase text-gray-500 mb-1">
							{tEntity(field.labelKey)}
						</span>
						<span className="text-gray-900 font-medium">
							{field.render(entity)}
						</span>
					</div>
				))}
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
