import { useTranslation } from "react-i18next";

interface ModalFooterProps {
	onCancel: () => void;
	isLoading: boolean;
	/** Already-translated confirm label (callers own their namespaces). */
	confirmLabel: string;
	/** Red confirm button for delete/remove dialogs. */
	destructive?: boolean;
	/**
	 * "submit" for dialogs whose body is a <form>; "button" for imperative
	 * dialogs, which must also pass `onConfirm`.
	 */
	confirmType?: "submit" | "button";
	onConfirm?: () => void;
}

/** The shared Cancel/Confirm footer used by every modal dialog. */
export const ModalFooter = ({
	onCancel,
	isLoading,
	confirmLabel,
	destructive = false,
	confirmType = "submit",
	onConfirm,
}: ModalFooterProps) => {
	const { t: tCommon } = useTranslation("common");

	return (
		<div className="flex justify-end gap-3 mt-6">
			<button
				type="button"
				onClick={onCancel}
				disabled={isLoading}
				className="px-4 py-2 bg-gray-400 hover:bg-gray-300 text-white rounded font-medium transition-colors"
			>
				{tCommon("Cancel")}
			</button>
			<button
				type={confirmType}
				onClick={confirmType === "button" ? onConfirm : undefined}
				disabled={isLoading}
				className={`px-4 py-2 ${
					destructive
						? "bg-red-600 hover:bg-red-700"
						: "bg-orange-500 hover:bg-orange-600"
				} font-medium text-white rounded transition-colors ${
					isLoading ? "opacity-50 cursor-not-allowed" : ""
				}`}
			>
				{isLoading ? tCommon("Processing") : confirmLabel}
			</button>
		</div>
	);
};
