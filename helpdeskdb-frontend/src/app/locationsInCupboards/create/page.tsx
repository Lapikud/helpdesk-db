"use client";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { LocationInCupboardService } from "@/services/LocationInCupboardService";
import { LocationService } from "@/services/LocationService";
import { CupboardService } from "@/services/CupboardService";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";
import { ILocation, ICupboard, ILocationInCupboardAdd } from "@/types/domain/DomainTypes";

export default function LocationInCupboardCreate() {
	const { t: tCupboardInCupboard } = useTranslation("locationincupboard");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [locationsData, setLocationsData] = useState<ILocation[]>([]);
	const [categoriesData, setCategoriesData] = useState<ICupboard[]>([]);
	const isAdmin = accountInfo?.roles?.includes("admins");

	const locationInCupboardService: LocationInCupboardService = useMemo(
		() => new LocationInCupboardService(),
		[]
	);
	const cupboardService: CupboardService = useMemo(
		() => new CupboardService(),
		[]
	);
	const locationService: LocationService = useMemo(() => new LocationService(), []);
	if (setAccountInfo) {
		locationInCupboardService.injectSetAccountInfo(setAccountInfo);
		cupboardService.injectSetAccountInfo(setAccountInfo);
		locationService.injectSetAccountInfo(setAccountInfo);
	}

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		if (!accountInfo?.jwt) {
			router.push("/login");
		}

		if (!isAdmin) {
			router.push("/");
			return;
		}

		const fetchData = async () => {
			try {
				const locationsInCupboardsResult =
					await locationInCupboardService.getAllAsync();
				if (locationsInCupboardsResult.errors) {
					console.log(locationsInCupboardsResult.errors);
					return;
				}

				const locationsResult = await locationService.getAllAsync();
				if (locationsResult.errors) {
					console.log(locationsResult.errors);
					return;
				}

				// Filter out locations that already have a cupboard
				const usedLocationIds = new Set(
					locationsInCupboardsResult.data!.map((ca) => ca.locationId)
				);
				const unusedLocations = locationsResult.data!.filter(
					(location) => !usedLocationIds.has(location.id)
				);
				setLocationsData(unusedLocations);

				const cupboardsResult = await cupboardService.getAllAsync();
				if (cupboardsResult.errors) {
					console.log(cupboardsResult.errors);
					return;
				}
				setCategoriesData(cupboardsResult.data!);
			} catch (error) {
				console.error("error fetching data: ", error);
			}
		};
		fetchData();
	}, [hydrated, accountInfo, router, isAdmin, setAccountInfo]);

	const [errorMessage, setErrorMessage] = useState("");

	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<ILocationInCupboardAdd>({});

	const onSubmit: SubmitHandler<ILocationInCupboardAdd> = async (
		data: ILocationInCupboardAdd
	) => {
		console.log(data);
		setErrorMessage("Loading...");
		try {
			const result = await locationInCupboardService.addAsync(
				data,
			);

			if (result.errors) {
				setErrorMessage(
					result.statusCode + " - " + result.errors.join(", ")
				);
				return;
			} else {
				setErrorMessage("");
				router.push("/locationsInCupboards");
			}
		} catch (error) {
			console.log("error: ", (error as Error).message);
			setErrorMessage((error as Error).message);
		}
	};

	if (!hydrated) {
		return <Spinner className="h-64" />;
	}

	if (!accountInfo?.jwt || !isAdmin) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-2">
				{tCommon("CreateTitle")}
			</h1>
			<h4 className="text-xl text-gray-700 mb-4">
				{tCupboardInCupboard("LocationInCupboardSingular")}
			</h4>
			<hr className="mb-6 border-gray-300" />

			<div className="max-w-md mx-auto">
				<form
					onSubmit={handleSubmit(onSubmit)}
					className="bg-white p-6 rounded-lg shadow-md space-y-5"
				>
					{errorMessage.length > 0 && (
						<p className="text-red-600">{errorMessage}</p>
					)}
					<div className="relative mb-4">
						<label
							className="block mb-1 text-sm font-medium text-gray-700"
							htmlFor="Location"
						>
							{tCupboardInCupboard("Location")}
						</label>
						<select
							id="locationId"
							{...register("locationId", {
								required: tValidation("Required", {
									field: tCupboardInCupboard("Location"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectAn")} {tCupboardInCupboard("Location")}
							</option>
							{locationsData.map((location) => (
								<option key={location.id} value={location.id}>
									{location.locationName}
								</option>
							))}
						</select>
						{errors.locationId && (
							<p className="mt-1 text-sm text-red-600">
								{errors.locationId.message}
							</p>
						)}
					</div>
					<div className="relative mb-4">
						<label
							className="block mb-1 text-sm font-medium text-gray-700"
							htmlFor="Cupboard"
						>
							{tCupboardInCupboard("Cupboard")}
						</label>
						<select
							id="cupboardId"
							{...register("cupboardId", {
								required: tValidation("Required", {
									field: tCupboardInCupboard("Cupboard"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectAn")}{" "}
								{tCupboardInCupboard("Cupboard")}
							</option>
							{categoriesData.map((cupboard) => (
								<option key={cupboard.id} value={cupboard.id}>
									{cupboard.codeName}
								</option>
							))}
						</select>
						{errors.cupboardId && (
							<p className="mt-1 text-sm text-red-600">
								{errors.cupboardId.message}
							</p>
						)}
					</div>


					<div>
						<button
							type="submit"
							className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-md transition"
						>
							{tCommon("CreateButton")}
						</button>
					</div>
				</form>
			</div>

			<div className="mt-6 text-center">
				<Link
					href="/locationsInCupboards"
					className="text-blue-600 hover:underline"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
