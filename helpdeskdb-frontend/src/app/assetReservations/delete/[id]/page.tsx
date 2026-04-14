"use client";

import Link from "next/link";
import { use, useContext, useEffect, useState } from "react";
import { AssetReservationService } from "@/services/AssetReservationService";
import { AssetService } from "@/services/AssetService";
import { UserService } from "@/services/UserService";
import { IAssetReservationWithNames } from "@/types/domain/DomainTypes";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import { useTranslation } from "react-i18next";
import Spinner from "@/components/LoadingSpinner";

export default function AssetReservationDelete({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tAssetReservation } = useTranslation("assetreservation");
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [errorMessage, setErrorMessage] = useState("");

	const [data, setData] = useState<IAssetReservationWithNames>();

	const assetReservationService = new AssetReservationService();
	const assetService = new AssetService();
	const userService = new UserService();

	if (setAccountInfo) {
		assetReservationService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
		userService.injectSetAccountInfo(setAccountInfo);
	}

	const isAdmin = accountInfo?.roles?.includes("admins");
	const isMember = accountInfo?.roles?.includes("members");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		if (!isAdmin && !isMember) {
			router.push("/assetReservations");
			return;
		}

		const fetchData = async () => {
			const result = await assetReservationService.getAsync(id);
			if (result.data) {
				if (new Date(result.data.reservationTo) < new Date()) {
					router.push("/assetReservations");
					return;
				}
				// Fetch joined data
				const [assetName, userResult] = await Promise.all([
					assetService.getAssetNameById(result.data.assetId),
					userService.getAsync(result.data.userId),
				]);

				setData({
					...result.data,
					assetName: assetName,
					userName: userResult.data
						? userResult.data.username
						: "Unknown User",
				});
			} else if (result.errors) {
				setErrorMessage(result.errors.join(", "));
			}
		};

		fetchData();
	}, [hydrated, router, id, isAdmin, isMember]);

	const deleteConfirmed = async () => {
		try {
			const result = await assetReservationService.deleteAsync(id);
			if (result.errors && result.errors.length > 0) {
				setErrorMessage(
					result.statusCode + " - " + result.errors.join(", "),
				);
			} else {
				setErrorMessage("");
				router.push("/assetReservations");
			}
		} catch (error) {
			console.error("error: ", error);
			setErrorMessage((error as Error).message);
		}
	};

	if (!hydrated || !data) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-2">
				{tCommon("DeleteTitle")}
			</h1>

			<h3 className="text-lg text-gray-700 mb-4">
				{tCommon("DeleteConfirmQuestion")}
			</h3>
			<div className="bg-white p-6 rounded-lg shadow-md max-w-xl mx-auto space-y-4">
				<h4 className="text-xl font-medium text-gray-800">
					{tAssetReservation("AssetReservationSingular")}
				</h4>
				<hr className="border-gray-300" />
				{errorMessage.length > 0 && (
					<p className="text-red-600">{errorMessage}</p>
				)}

				<dl className="space-y-4">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tAssetReservation("AssetId")}
						</dt>
						<dd className="w-2/3 text-gray-900">
							{data.assetName}
						</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tAssetReservation("UserId")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.userName}</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tAssetReservation("ReservationFrom")}
						</dt>
						<dd className="w-2/3 text-gray-900">
							{new Date(data.reservationFrom).toLocaleString()}
						</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tAssetReservation("ReservationTo")}
						</dt>
						<dd className="w-2/3 text-gray-900">
							{new Date(data.reservationTo).toLocaleString()}
						</dd>
					</div>
				</dl>

				<div className="mt-6 flex items-center space-x-4 justify-center">
					<button
						onClick={() => deleteConfirmed()}
						type="button"
						title="Delete"
						className="bg-red-600 hover:bg-red-700 text-white font-semibold py-2 px-4 rounded transition"
					>
						{tCommon("DeleteButton")}
					</button>

					<span className="text-gray-400">|</span>

					<Link
						href="/assetReservations"
						className="text-blue-600 hover:underline font-medium"
					>
						{tCommon("BackToListLink")}
					</Link>
				</div>
			</div>
		</>
	);
}
