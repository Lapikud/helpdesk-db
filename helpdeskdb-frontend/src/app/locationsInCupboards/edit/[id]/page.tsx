"use client";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { LocationInCupboardService } from "@/services/LocationInCupboardService";
import { LocationService } from "@/services/LocationService";
import { CupboardService } from "@/services/CupboardService";
import { ILocation, ICupboard, ILocationInCupboard } from "@/types/domain/DomainTypes";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";

export default function CupboardLocationsEdit({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tCupboardLocations } = useTranslation("locationINcupboard");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [isLoading, setIsLoading] = useState(true);
	const [errorMessage, setErrorMessage] = useState("");

	const [locationsData, setLocationsData] = useState<ILocation[]>([]);
	const [categoriesData, setCupboardsData] = useState<ICupboard[]>([]);

	const cupboardLocationsService: LocationInCupboardService = useMemo(
		() => new LocationInCupboardService(),
		[]
	);
	const locationService: LocationService = useMemo(() => new LocationService(), []);
	const cupboardService: CupboardService = useMemo(
		() => new CupboardService(),
		[]
	);
	if (setAccountInfo) {
		cupboardLocationsService.injectSetAccountInfo(setAccountInfo);
		locationService.injectSetAccountInfo(setAccountInfo);
		cupboardService.injectSetAccountInfo(setAccountInfo);
	}

	const isAdmin = accountInfo?.roles?.includes("admins");


	const {
		register,
		handleSubmit,
		formState: { errors },
		reset,
		setValue,
	} = useForm<ILocationInCupboard>();

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
				setIsLoading(true);

				// get this CupboardLocation
				const thisCupboardLocations = await cupboardLocationsService.getAsync(
					id
				);

				if (thisCupboardLocations.errors || !thisCupboardLocations.data) {
					setErrorMessage(
						thisCupboardLocations.errors?.join(", ") ||
							"Failed to load cupboard locations"
					);
					return;
				}

				// get all locations
				const locationsResult = await locationService.getAllAsync();
				if (locationsResult.errors) {
					console.log(locationsResult.errors);
					return;
				}

				// set locationsData to this current cupboard
				setLocationsData(
					locationsResult.data!.filter(
						(u) => u.id === thisCupboardLocations.data!.locationId
					)
				);

				// get all categoreis
				const categoriesResult = await cupboardService.getAllAsync();
				if (categoriesResult.errors) {
					console.log(categoriesResult.errors);
					return;
				}

				setCupboardsData(categoriesResult.data!);

				reset({
					cupboardId: thisCupboardLocations.data.cupboardId,
					locationId: thisCupboardLocations.data.locationId,
				});
			} catch (error) {
				console.error("error fetching data: ", error);
			} finally {
				setIsLoading(false);
			}
		};
		fetchData();
	}, [
		hydrated,
		accountInfo,
		router,
		id,
		reset,
		isAdmin,
		locationService,
		cupboardService,
	]);

	const onSubmit: SubmitHandler<ILocationInCupboard> = async (
		data: ILocationInCupboard
	) => {
		setErrorMessage("Loading...");
		try {
			const result = await cupboardLocationsService.updateAsync({
				id: id,
				locationId: data.locationId,
				cupboardId: data.cupboardId,
			});
			console.log("edit result", result);

			if (result.errors && result.errors.length > 0) {
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

	if (!hydrated || isLoading) {
		return <Spinner className="h-64" />;
	}

	if (!accountInfo?.jwt || !isAdmin) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-2">
				{tCommon("EditTitle")}
			</h1>
			<h4 className="text-xl text-gray-700 mb-4">
				{tCupboardLocations("LocationInCupboardSingular")}
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
							{tCupboardLocations("Location")}
						</label>
						<select
							id="locationId"
							{...register("locationId", {
								required: tValidation("Required", {
									field: tCupboardLocations("Location"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectAn")} {tCupboardLocations("Location")}
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
							htmlFor="As"
						>
							{tCupboardLocations("Cupboard")}
						</label>
						<select
							id="cupboardId"
							{...register("cupboardId", {
								required: tValidation("Required", {
									field: tCupboardLocations("Cupboard"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectA")}{" "}
								{tCupboardLocations("Cupboard")}
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
							{tCommon("EditLink")}
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
