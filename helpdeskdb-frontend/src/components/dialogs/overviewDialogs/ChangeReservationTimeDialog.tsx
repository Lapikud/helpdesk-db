import { IAssetViewModel } from "@/types/domain/IAssetViewModels";
import { useTranslation } from "react-i18next";
import { useContext, useState, useEffect } from "react";
import { Modal } from "../common/Modal";

import { IAssetReservationUpdate } from "@/types/domain/DomainTypes";
import { AccountContext } from "@/context/AccountContext";
import { DateTimePicker } from "@/components/DateTimePicker";

interface ChangeReservationTimeDialogProps {
	open: boolean;
	assetReservationId: string | null;
	asset: IAssetViewModel | null;
	initialFrom: Date | null;
	initialTo: Date | null;
	onClose: () => void;
	onConfirm: (
		assetReservationId: string,
		data: IAssetReservationUpdate,
	) => Promise<void>;
	isLoading: boolean;
}

export const ChangeReservationTimeDialog = ({
	open,
	assetReservationId,
	asset,
	initialFrom,
	initialTo,
	onClose,
	onConfirm,
	isLoading,
}: ChangeReservationTimeDialogProps) => {
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { t: tChangeReservationTime } = useTranslation("changeReservationTime");
	const { accountInfo } = useContext(AccountContext);

	const [reservationFrom, setReservationFrom] = useState<Date | null>(null);
	const [reservationTo, setReservationTo] = useState<Date | null>(null);

	useEffect(() => {
		if (open) {
			setReservationFrom(initialFrom);
			setReservationTo(initialTo);
		}
	}, [open, initialFrom, initialTo]);

	const handleClose = () => {
		onClose();
	};

	const handleSubmit = async () => {
		if (!asset || !assetReservationId) return;

		if (!reservationFrom || !reservationTo) {
			alert(tValidation("BothDatesRequired"));
			return;
		}

		if (reservationTo <= reservationFrom) {
			alert(tValidation("ReservationToMustBeAfterFrom"));
			return;
		}

		try {
			await onConfirm(assetReservationId, {
				assetReservationId,
				assetId: asset.id,
				userId: accountInfo?.id ?? "",
				reservationFrom: reservationFrom,
				reservationTo: reservationTo,
			});
			setReservationFrom(null);
			setReservationTo(null);
			handleClose();
		} catch (error) {
			console.error("Change reservation time failed:", error);
		}
	};

	if (!asset || !assetReservationId) return null;

	return (
		<Modal open={open} onClose={handleClose}>
			<h3 className="font-bold text-xl mb-2 text-black">
				{tChangeReservationTime("Title")}
			</h3>
			<h3 className="font-bold text-lg mb-2 text-black">
				{asset.assetName}
			</h3>
			<div className="mb-4">
				<label className="block text-gray-700 mb-2">
					{tChangeReservationTime("NewReservationFrom")}
				</label>
				<DateTimePicker
					value={reservationFrom}
					onChange={setReservationFrom}
				/>
			</div>
			<div className="mb-4">
				<label className="block text-gray-700 mb-2">
					{tChangeReservationTime("NewReservationTo")}
				</label>
				<DateTimePicker
					value={reservationTo}
					onChange={setReservationTo}
				/>
			</div>
			<div className="flex justify-end gap-3">
				<button
					onClick={handleClose}
					className="px-4 py-2 bg-gray-400 hover:bg-gray-300 rounded"
					disabled={isLoading}
				>
					{tCommon("Cancel")}
				</button>
				<button
					onClick={handleSubmit}
					disabled={isLoading}
					className={`px-4 py-2 bg-orange-500 hover:bg-orange-600 text-white rounded ${
						isLoading ? "opacity-50 cursor-not-allowed" : ""
					}`}
				>
					{isLoading ? tCommon("Processing") : tChangeReservationTime("UpdateTimeButton")}
				</button>
			</div>
		</Modal>
	);
};
