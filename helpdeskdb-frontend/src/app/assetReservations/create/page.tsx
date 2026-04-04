"use client";

import Link from "next/link";
import { useContext, useEffect, useState } from "react";
import { AssetReservationService } from "@/services/AssetReservationService";
import { AssetService } from "@/services/AssetService";
import { UserService } from "@/services/UserService";
import { IAssetReservationAdd, IAsset, IUser } from "@/types/domain/DomainTypes";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import { useTranslation } from "react-i18next";
import { DateTimePicker } from "@/components/DateTimePicker";
import Spinner from "@/components/LoadingSpinner";

export default function AssetReservationCreate() {
	const { t: tAssetReservation } = useTranslation("assetreservation");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	
    const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);

	const isAdmin = accountInfo?.roles?.includes("admins");
	const isMember = accountInfo?.roles?.includes("members");

	const [assets, setAssets] = useState<IAsset[]>([]);
	const [users, setUsers] = useState<IUser[]>([]);

    const [selectedAssetId, setSelectedAssetId] = useState<string>("");
    const [selectedUserId, setSelectedUserId] = useState<string>("");
    const [reservationFrom, setReservationFrom] = useState<Date | null>(null);
    const [reservationTo, setReservationTo] = useState<Date | null>(null);

	const [errorMessage, setErrorMessage] = useState("");
	const [isLoading, setIsLoading] = useState(false);

	const assetReservationService = new AssetReservationService();
	const assetService = new AssetService();
	const userService = new UserService();

	if (setAccountInfo) {
		assetReservationService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
		userService.injectSetAccountInfo(setAccountInfo);
	}

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		if (!accountInfo?.jwt) {
			router.push("/login");
			return;
		}
		if (!isAdmin) {
			router.push("/assetReservations");
            return;
		}

		const fetchDependencies = async () => {
			const [assetsRes, usersRes] = await Promise.all([
				assetService.getAllAsync(),
				userService.getAllAsync(),
			]);
			
			if (assetsRes.data) setAssets(assetsRes.data);
			if (usersRes.data) setUsers(usersRes.data);
            
            // Default select the logged in user to make form easier to use for staff
            if (accountInfo?.id) {
                setSelectedUserId(accountInfo.id);
            }
		};

		fetchDependencies();
	}, [hydrated, accountInfo, router, isAdmin]);


	const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
		setErrorMessage("");

		// Manual Validation since we are not using react-hook-form here to better support DateTimePicker
		if (!selectedAssetId) {
			setErrorMessage(tValidation("Required", { field: tAssetReservation("AssetId") }));
			return;
		}
		if (!selectedUserId) {
			setErrorMessage(tValidation("Required", { field: tAssetReservation("UserId") }));
			return;
		}
		if (!reservationFrom || !reservationTo) {
			setErrorMessage(tValidation("BothDatesRequired"));
			return;
		}
		if (reservationTo <= reservationFrom) {
			setErrorMessage(tValidation("ReservationToMustBeAfterFrom"));
			return;
		}

        setIsLoading(true);

		try {
			const result = await assetReservationService.addAsync({
				assetId: selectedAssetId,
				userId: selectedUserId,
				reservationFrom: reservationFrom.toISOString(),
                reservationTo: reservationTo.toISOString(),
			});

			if (result.errors) {
				console.log(result);
				setErrorMessage(result.errors?.join(", ") || "Failed to reserve asset");
			} else {
				router.push("/assetReservations");
			}
		} catch (error) {
			console.error("error: ", error);
			setErrorMessage((error as Error).message);
		} finally {
            setIsLoading(false);
        }
	};

	if (!hydrated || (!accountInfo?.jwt) || (!isAdmin && !isMember)) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-2">
				{tCommon("CreateTitle")}
			</h1>
			<h4 className="text-xl text-gray-700 mb-4">
				{tAssetReservation("AssetReservationSingular")}
			</h4>
			<hr className="mb-6 border-gray-300" />

			<div className="max-w-md mx-auto">
				<form
					onSubmit={handleSubmit}
					className="bg-white p-6 rounded-lg shadow-md space-y-5"
				>
					{errorMessage.length > 0 && (
						<p className="text-red-600">{errorMessage}</p>
					)}
					
                    <div className="relative mb-4">
						<label className="block mb-1 text-sm font-medium text-gray-700">
							{tAssetReservation("AssetId")}
						</label>
						<select
							className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base shadow-sm focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
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

                    <div className="relative mb-4">
						<label className="block mb-1 text-sm font-medium text-gray-700">
							{tAssetReservation("UserId")}
						</label>
						<select
							className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base shadow-sm focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
							value={selectedUserId}
                            onChange={(e) => setSelectedUserId(e.target.value)}
						>
							<option value="">{tAssetReservation("UserIdPrompt")}</option>
							{users.map((u) => (
								<option key={u.id} value={u.id}>
									{u.username}
								</option>
							))}
						</select>
					</div>

                    <div className="relative mb-4">
						<label className="block mb-1 text-sm font-medium text-gray-700">
							{tAssetReservation("ReservationFrom")}
						</label>
						<DateTimePicker
                            value={reservationFrom}
                            onChange={setReservationFrom}
                        />
					</div>

                    <div className="relative mb-4">
						<label className="block mb-1 text-sm font-medium text-gray-700">
							{tAssetReservation("ReservationTo")}
						</label>
						<DateTimePicker
                            value={reservationTo}
                            onChange={setReservationTo}
                        />
					</div>

					<div>
						<button
							type="submit"
                            disabled={isLoading}
							className={`w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-md transition ${isLoading ? "opacity-50 cursor-not-allowed" : ""}`}
						>
							{isLoading ? "..." : tCommon("CreateButton")}
						</button>
					</div>
				</form>
			</div>

			<div className="mt-6 text-center">
				<Link
					href="/assetReservations"
					className="text-blue-600 hover:underline"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
