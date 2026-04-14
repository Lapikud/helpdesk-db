"use client";

import Link from "next/link";
import { useContext, useEffect, useState } from "react";
import { LocationService } from "@/services/LocationService";
import { ILocationAdd } from "@/types/domain/DomainTypes";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import { useTranslation } from "react-i18next";
import Spinner from "@/components/LoadingSpinner";

export default function LocationCreate() {
	const { t: tLocation } = useTranslation("location");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);

	const isAdmin = accountInfo?.roles?.includes("admins");


	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		if (!isAdmin) {
			router.push("/");
		}
	}, [hydrated, router, isAdmin]);

	const [errorMessage, setErrorMessage] = useState("");

	const locationService: LocationService = new LocationService();

	if (setAccountInfo) {
		locationService.injectSetAccountInfo(setAccountInfo);
	}

	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<ILocationAdd>({});

	const onSubmit: SubmitHandler<ILocationAdd> = async (
		data: ILocationAdd
	) => {
		console.log(data);
		setErrorMessage("Loading...");

		try {
			const result = await locationService.addAsync({
				locationName: data.locationName,
				shelfNum: data.shelfNum,
				column: data.column
			});

			if (result.errors) {
				setErrorMessage(
					result.statusCode + " - " + result.errors.join(", ")
				);
				return;
			} else {
				setErrorMessage("");
				router.push("/locations");
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
				{tLocation("LocationSingular")}
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
							htmlFor="LocationName"
						>
							{tLocation("LocationName")}
						</label>
						<input
							className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base shadow-sm placeholder-gray-400 focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
							type="text"
							id="locationName"
							placeholder={tLocation("LocationNamePrompt")}
							{...register("locationName", {
								required: {
									value: true,
									message: tValidation("Required", {
										field: tLocation("LocationName"),
									}),
								},
								minLength: {
									value: 2,
									message: tValidation(
										"MinLenghtValidationError",
										{
											field: tLocation("LocationName"),
											min: 2,
										}
									),
								},
								maxLength: {
									value: 128,
									message: tValidation(
										"MaxLengthValidationError",
										{
											field: tLocation("LocationName"),
											max: 128,
										}
									),
								},
							})}
						/>
						{errors.locationName && (
							<span className="text-red-600 text-sm">
								{errors.locationName.message}
							</span>
						)}
					</div>

					<div className="relative mb-4">
						<label
							className="block mb-1 text-sm font-medium text-gray-700"
							htmlFor="shelfNum"
						>
							{tLocation("ShelfNum")}
						</label>
						<input
							className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base shadow-sm placeholder-gray-400 focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
							type="number"
							placeholder="1"
							id="shelfNum"
							{...register("shelfNum", {
								required: {
									value: true,
									message: tValidation("Required", {
										field: tLocation("ShelfNum"),
									}),
								},
								min: {
									value: 1,
									message: tValidation(
										"ValueBetween",
										{
											field: tLocation("ShelfNum"),
											min: 1,
											max: 5,
										}
									),
								},
								max: {
									value: 5,
									message: tValidation(
										"ValueBetween",
										{
											field: tLocation("ShelfNum"),
											min: 1,
											max: 5,
										}
									),
								},
							})}
						/>
						{errors.shelfNum && (
							<span className="text-red-600 text-sm">
								{errors.shelfNum.message}
							</span>
						)}
					</div>

					<div className="relative mb-4">
						<label
							className="block mb-1 text-sm font-medium text-gray-700"
							htmlFor="column"
						>
							{tLocation("Column")}
						</label>
						<input
							className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base shadow-sm placeholder-gray-400 focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
							type="number"
							placeholder="1"
							id="column"
							{...register("column", {
								required: {
									value: true,
									message: tValidation("Required", {
										field: tLocation("Column"),
									}),
								},
								min: {
									value: 1,
									message: tValidation(
										"ValueBetween",
										{
											field: tLocation("Column"),
											min: 1,
											max: 5,
										}
									),
								},
								max: {
									value: 5,
									message: tValidation(
										"ValueBetween",
										{
											field: tLocation("Column"),
											min: 1,
											max: 5,
										}
									),
								},
							})}
						/>
						{errors.column && (
							<span className="text-red-600 text-sm">
								{errors.column.message}
							</span>
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
					href="/locations"
					className="text-blue-600 hover:underline"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
