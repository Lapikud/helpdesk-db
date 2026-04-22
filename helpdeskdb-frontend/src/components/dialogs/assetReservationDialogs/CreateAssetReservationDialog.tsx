import { useContext, useState } from "react";
import { useTranslation } from "react-i18next";
import { Modal } from "../common/Modal";
import { DateTimePicker } from "@/components/DateTimePicker";
import { AccountContext } from "@/context/AccountContext";
import { IAsset, IAssetReservationAdd } from "@/types/domain/DomainTypes";

interface CreateAssetReservationDialogProps {
	open: boolean;
	assets: IAsset[];
	onClose: () => void;
	onConfirm: (
		data: IAssetReservationAdd,
	) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const CreateAssetReservationDialog = ({
	open,
	assets,
	onClose,
	onConfirm,
	isLoading,
}: CreateAssetReservationDialogProps) => {
	const { t: tAssetReservation } = useTranslation("assetreservation");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { accountInfo } = useContext(AccountContext);

	const [selectedAssetId, setSelectedAssetId] = useState("");
	const [reservationFrom, setReservationFrom] = useState<Date | null>(null);
	const [reservationTo, setReservationTo] = useState<Date | null>(null);
	const [errorMsg, setErrorMsg] = useState<string | null>(null);

	const resetForm = () => {
		setSelectedAssetId("");
		setReservationFrom(null);
		setReservationTo(null);
		setErrorMsg(null);
	};

	const handleClose = () => {
		resetForm();
		onClose();
	};

	const handleSubmit = async () => {
		setErrorMsg(null);

		if (!accountInfo?.id) {
			setErrorMsg(tValidation("Required", { field: tAssetReservation("UserId") }));
			return;
		}
		if (!selectedAssetId) {
			setErrorMsg(tValidation("Required", { field: tAssetReservation("AssetId") }));
			return;
		}
		if (!reservationFrom || !reservationTo) {
			setErrorMsg(tValidation("BothDatesRequired"));
			return;
		}
		if (reservationTo <= reservationFrom) {
			setErrorMsg(tValidation("ReservationToMustBeAfterFrom"));
			return;
		}

		const result = await onConfirm({
			assetId: selectedAssetId,
			userId: accountInfo.id,
			reservationFrom: reservationFrom.toISOString(),
			reservationTo: reservationTo.toISOString(),
		});

		if (result && result.error) {
			setErrorMsg(result.error);
			return;
		}

		resetForm();
	};

	return (
		<Modal open={open} onClose={handleClose}>
			<h2 className="text-xl font-bold mb-2 text-black">
				{tCommon("CreateTitle")}
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
				<select
					className="w-full p-2 border border-gray-300 rounded"
					value={selectedAssetId}
					onChange={(e) => setSelectedAssetId(e.target.value)}
				>
					<option value="">{tAssetReservation("AssetIdPrompt")}</option>
					{assets.map((a) => (
						<option key={a.id} value={a.id}>
							{a.assetName}
						</option>
					))}
				</select>
			</div>

			<div className="mb-4">
				<label className="block text-gray-700 mb-2">
					{tAssetReservation("UserId")}
				</label>
				<div className="w-full p-2 border border-gray-200 rounded bg-gray-50 text-gray-700">
					{accountInfo?.name ?? ""}
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
					{isLoading ? tCommon("Processing") : tCommon("CreateButton")}
				</button>
			</div>
		</Modal>
	);
};
