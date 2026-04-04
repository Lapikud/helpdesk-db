import { useTranslation } from "react-i18next";
import { Modal } from "../common/Modal";
import { IAssetReservationWithNames } from "@/types/domain/DomainTypes";

interface RemoveReservationDialogProps {
	open: boolean;
	reservation: IAssetReservationWithNames | null;
	onClose: () => void;
	onConfirm: (reservationId: string) => Promise<void>;
	isLoading: boolean;
}

export const RemoveReservationDialog = ({
	open,
	reservation,
	onClose,
	onConfirm,
	isLoading,
}: RemoveReservationDialogProps) => {
	const { t: tCommon } = useTranslation("common");
	const { t: tRemoveRes } = useTranslation("removeReservation");
	const { t: tAssetRes } = useTranslation("assetreservation");

	const handleClose = () => {
		onClose();
	};

	const handleSubmit = async () => {
		if (!reservation) return;

		try {
			await onConfirm(reservation.assetId);
			handleClose();
		} catch (error) {
			console.error("Remove reservation failed:", error);
		}
	};

	if (!reservation) return null;

	const formatDate = (date: Date | string | undefined) => {
		if (!date) return "";
		const d = new Date(date);
		return `${d.toLocaleDateString()} ${d.toLocaleTimeString([], {
			hour: "2-digit",
			minute: "2-digit",
		})}`;
	};

	return (
		<Modal open={open} onClose={handleClose}>
			<h3 className="font-bold text-xl mb-4 text-black">
				{tRemoveRes("Title")}
			</h3>
			<p className="text-gray-700 mb-6">
				{tRemoveRes("ConfirmQuestion")}
			</p>
			<h4 className="font-bold text-lg mb-4 text-black">
				{reservation.assetName}
			</h4>

			<div className="bg-gray-50 p-4 rounded-lg mb-6 border border-gray-100">
				<div className="grid grid-cols-2 gap-4">
					<div>
						<span className="block text-xs font-bold uppercase text-gray-500 mb-1">
							{tAssetRes("ReservationFrom")}
						</span>
						<span className="text-gray-900 font-medium">
							{formatDate(reservation.reservationFrom)}
						</span>
					</div>
					<div>
						<span className="block text-xs font-bold uppercase text-gray-500 mb-1">
							{tAssetRes("ReservationTo")}
						</span>
						<span className="text-gray-900 font-medium">
							{formatDate(reservation.reservationTo)}
						</span>
					</div>
				</div>
			</div>

			<div className="flex justify-end gap-3 mt-8">
				<button
					onClick={handleClose}
					className="px-4 py-2 bg-gray-400 hover:bg-gray-300 text-white rounded font-medium transition-colors"
					disabled={isLoading}
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
					{isLoading
						? tCommon("Processing")
						: tRemoveRes("ConfirmButton")}
				</button>
			</div>
		</Modal>
	);
};
