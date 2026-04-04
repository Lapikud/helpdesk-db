import { IAssetViewModel } from "@/types/domain/IAssetViewModels";
import { useTranslation } from "react-i18next";
import { useContext, useState } from "react";
import { Modal } from "../common/Modal";

import { IAssetReservationAdd } from "@/types/domain/DomainTypes";
import { AccountContext } from "@/context/AccountContext";
import { DateTimePicker } from "@/components/DateTimePicker";

interface ReserveAssetDialogProps {
	open: boolean;
	asset: IAssetViewModel | null;
	onClose: () => void;
	onConfirm: (
		assetId: string,
		ReserveAsset: IAssetReservationAdd,
	) => Promise<{ success: boolean; error?: string } | void>;
	isLoading: boolean;
}

export const ReserveAssetDialog = ({
	open,
	asset,
	onClose,
	onConfirm,
	isLoading,
}: ReserveAssetDialogProps) => {
	const { t: tCommon } = useTranslation("common");
	const { t: tReserve } = useTranslation("Reserve");
	const { t: tAssetReservation } = useTranslation("assetreservation");
	const { t: tValidation } = useTranslation("validationerrors");
	const { accountInfo } = useContext(AccountContext);

	const [reservationFrom, setReservationFrom] = useState<Date | null>(null);
	const [reservationTo, setReservationTo] = useState<Date | null>(null);
	const [errorMsg, setErrorMsg] = useState<string | null>(null);

	const handleClose = () => {
		setErrorMsg(null);
		setReservationFrom(null);
		setReservationTo(null);
		onClose();
	};

	const handleSubmit = async () => {
		setErrorMsg(null);
		if (!asset) return;

		if (!reservationFrom || !reservationTo) {
			setErrorMsg(tValidation("BothDatesRequired"));
			return;
		}

		if (reservationTo <= reservationFrom) {
			setErrorMsg(tValidation("ReservationToMustBeAfterFrom"));
			return;
		}

		try {
			const result = await onConfirm(asset.id, {
				assetId: asset.id,
				userId: accountInfo?.id ?? "",
				reservationFrom: reservationFrom.toISOString(),
				reservationTo: reservationTo.toISOString(),
			});

			if (result && result.success === false) {
				setErrorMsg(result.error || "Reservation failed");
				return;
			}

			setReservationFrom(null);
			setReservationTo(null);
			handleClose();
		} catch (error) {
			console.error("Reserve failed:", error);
			setErrorMsg("Reserve failed unexpectedly");
		}
	};

	if (!asset) return null;

	return (
		<Modal open={open} onClose={handleClose}>
			<h3 className="font-bold text-xl mb-2 text-black">
				{tReserve("ReserveConfirmQuestion")}
			</h3>
			<h3 className="font-bold text-lg mb-4 text-black">
				{asset.assetName}
			</h3>
			{errorMsg && (
				<div className="mb-4 p-3 bg-red-100 text-red-700 border border-red-400 rounded">
					{errorMsg}
				</div>
			)}
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
				<DateTimePicker
					value={reservationTo}
					onChange={setReservationTo}
				/>
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
					{isLoading
						? tCommon("Processing")
						: tReserve("ReserveButton")}
				</button>
			</div>
		</Modal>
	);
};
