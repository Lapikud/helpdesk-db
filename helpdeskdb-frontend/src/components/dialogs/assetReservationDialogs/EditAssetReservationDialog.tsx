import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../common/Modal";
import { DateTimePicker } from "@/components/DateTimePicker";
import {
	IAssetReservation,
	IAssetReservationWithNames,
} from "@/types/domain/DomainTypes";

interface EditAssetReservationDialogProps {
	open: boolean;
	reservation: IAssetReservationWithNames | null;
	onClose: () => void;
	onConfirm: (
		data: IAssetReservation,
	) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const EditAssetReservationDialog = ({
	open,
	reservation,
	onClose,
	onConfirm,
	isLoading,
}: EditAssetReservationDialogProps) => {
	const { t: tAssetReservation } = useTranslation("assetreservation");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");

	const [reservationFrom, setReservationFrom] = useState<Date | null>(null);
	const [reservationTo, setReservationTo] = useState<Date | null>(null);
	const [errorMsg, setErrorMsg] = useState<string | null>(null);

	useEffect(() => {
		if (open && reservation) {
			setReservationFrom(new Date(reservation.reservationFrom));
			setReservationTo(new Date(reservation.reservationTo));
			setErrorMsg(null);
		}
	}, [open, reservation]);

	const handleClose = () => {
		setErrorMsg(null);
		onClose();
	};

	const handleSubmit = async () => {
		setErrorMsg(null);
		if (!reservation) return;

		if (!reservationFrom || !reservationTo) {
			setErrorMsg(tValidation("BothDatesRequired"));
			return;
		}
		if (reservationTo <= reservationFrom) {
			setErrorMsg(tValidation("ReservationToMustBeAfterFrom"));
			return;
		}

		const result = await onConfirm({
			id: reservation.id,
			assetId: reservation.assetId,
			userId: reservation.userId,
			reservationFrom: reservationFrom.toISOString() as unknown as Date,
			reservationTo: reservationTo.toISOString() as unknown as Date,
			isReturned: false,
		});

		if (result && result.error) {
			setErrorMsg(result.error);
			return;
		}
	};

	if (!reservation) return null;

	return (
		<Modal open={open} onClose={handleClose}>
			<h2 className="text-xl font-bold mb-2 text-black">
				{tCommon("EditTitle")}
			</h2>
			<h4 className="text-lg text-gray-700 mb-4">
				{tAssetReservation("AssetReservationSingular")}
			</h4>

			{errorMsg && (
				<div className="mb-4 p-3 bg-red-100 text-red-700 border border-red-400 rounded">
					{errorMsg}
				</div>
			)}

			<div className="mb-4">
				<label className="block text-gray-700 mb-2">
					{tAssetReservation("AssetId")}
				</label>
				<div className="w-full p-2 border border-gray-200 rounded bg-gray-50 text-gray-700">
					{reservation.assetName}
				</div>
			</div>

			<div className="mb-4">
				<label className="block text-gray-700 mb-2">
					{tAssetReservation("UserId")}
				</label>
				<div className="w-full p-2 border border-gray-200 rounded bg-gray-50 text-gray-700">
					{reservation.userName}
				</div>
			</div>

			<div className="mb-4">
				<label className="block text-gray-700 mb-2">
					{tAssetReservation("ReservationFrom")}
				</label>
				<DateTimePicker
					value={reservationFrom}
					onChange={setReservationFrom}
				/>
			</div>

			<div className="mb-4">
				<label className="block text-gray-700 mb-2">
					{tAssetReservation("ReservationTo")}
				</label>
				<DateTimePicker value={reservationTo} onChange={setReservationTo} />
			</div>

			<div className="flex justify-end gap-3 mt-6">
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
					className={`px-4 py-2 bg-orange-500 hover:bg-orange-600 font-medium text-white rounded transition-colors ${
						isLoading ? "opacity-50 cursor-not-allowed" : ""
					}`}
				>
					{isLoading ? tCommon("Processing") : tCommon("SaveButton")}
				</button>
			</div>
		</Modal>
	);
};
