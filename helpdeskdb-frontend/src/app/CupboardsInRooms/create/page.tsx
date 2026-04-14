"use client";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { CupboardsInRoomsService } from "@/services/CupboardsInRoomsService";
import { RoomService } from "@/services/RoomService";
import { CupboardService } from "@/services/CupboardService";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";
import { IRoom, ICupboard, ICupboardInRoomAdd } from "@/types/domain/DomainTypes";

export default function CupboardsInRoomsCreate() {
	const { t: tCupboardInRoom } = useTranslation("cupboardinroom");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [roomsData, setRoomsData] = useState<IRoom[]>([]);
	const [categoriesData, setCategoriesData] = useState<ICupboard[]>([]);
	const isAdmin = accountInfo?.roles?.includes("admins");

	const cupboardsInRoomsService: CupboardsInRoomsService = useMemo(
		() => new CupboardsInRoomsService(),
		[]
	);
	const cupboardService: CupboardService = useMemo(
		() => new CupboardService(),
		[]
	);
	const roomService: RoomService = useMemo(() => new RoomService(), []);
	if (setAccountInfo) {
		cupboardsInRoomsService.injectSetAccountInfo(setAccountInfo);
		cupboardService.injectSetAccountInfo(setAccountInfo);
		roomService.injectSetAccountInfo(setAccountInfo);
	}

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		if (!isAdmin) {
			router.push("/");
			return;
		}

		const fetchData = async () => {
			try {
				const cupboardsInRoomsResult =
					await cupboardsInRoomsService.getAllAsync();
				if (cupboardsInRoomsResult.errors) {
					console.log(cupboardsInRoomsResult.errors);
					return;
				}

				const cupboardsResult = await cupboardService.getAllAsync();
				if (cupboardsResult.errors) {
					console.log(cupboardsResult.errors);
					return;
				}

				// Filter out cupboards that already have a room
				const usedCupboardIds = new Set(
					cupboardsInRoomsResult.data!.map((ca) => ca.cupboardId)
				);
				const unusedCupboards = cupboardsResult.data!.filter(
					(cupboard) => !usedCupboardIds.has(cupboard.id)
				);
				setCategoriesData(unusedCupboards);

				const roomsResult = await roomService.getAllAsync();
				if (roomsResult.errors) {
					console.log(roomsResult.errors);
					return;
				}
				setRoomsData(roomsResult.data!);
			} catch (error) {
				console.error("error fetching data: ", error);
			}
		};
		fetchData();
	}, [hydrated, router, isAdmin, cupboardsInRoomsService, cupboardService, roomService]);

	const [errorMessage, setErrorMessage] = useState("");

	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<ICupboardInRoomAdd>({});

	const onSubmit: SubmitHandler<ICupboardInRoomAdd> = async (
		data: ICupboardInRoomAdd
	) => {
		console.log(data);
		setErrorMessage("Loading...");
		try {
			const result = await cupboardsInRoomsService.addAsync(data,);

			if (result.errors) {
				setErrorMessage(
					result.statusCode + " - " + result.errors.join(", ")
				);
				return;
			} else {
				setErrorMessage("");
				router.push("/cupboardsInRooms");
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
				{tCupboardInRoom("CupboardsInRooms")}
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
							htmlFor="Cupboard"
						>
							{tCupboardInRoom("Cupboard")}
						</label>
						<select
							id="cupboardId"
							{...register("cupboardId", {
								required: tValidation("Required", {
									field: tCupboardInRoom("Cupboard"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectAn")}{" "}
								{tCupboardInRoom("Cupboard")}
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

					<div className="relative mb-4">
						<label
							className="block mb-1 text-sm font-medium text-gray-700"
							htmlFor="Room"
						>
							{tCupboardInRoom("Room")}
						</label>
						<select
							id="roomId"
							{...register("roomId", {
								required: tValidation("Required", {
									field: tCupboardInRoom("Room"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectAn")} {tCupboardInRoom("Room")}
							</option>
							{roomsData.map((room) => (
								<option key={room.id} value={room.id}>
									{room.roomName}
								</option>
							))}
						</select>
						{errors.roomId && (
							<p className="mt-1 text-sm text-red-600">
								{errors.roomId.message}
							</p>
						)}
					</div>


					<div className="relative">
						<label
							className="block mb-1 text-sm font-medium text-gray-700"
							htmlFor="Comment"
						>
							{tCommon("Comment")}
						</label>
						<input
							className="block w-full rounded-md border border-gray-300 px-4 py-3 text-base shadow-sm placeholder-gray-400 focus:border-blue-500 focus:ring focus:ring-blue-300 focus:ring-opacity-50"
							type="text"
							id="Comment"
							placeholder={tCommon("CommentPrompt")}
							{...register("comment", {
								required: {
									value: true,
									message: tValidation("Required", {
										field: tCommon("Comment"),
									}),
								},
								minLength: {
									value: 2,
									message: tValidation(
										"MinLenghtValidationError",
										{
											field: tCommon("Comment"),
											min: 2,
										}
									),
								},
								maxLength: {
									value: 255,
									message: tValidation(
										"MaxLengthValidationError",
										{
											field: tCommon("Comment"),
											max: 255,
										}
									),
								},
							})}
						/>
						{errors.comment && (
							<span className="text-red-600 text-sm">
								{errors.comment.message}
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
					href="/cupboardsInRooms"
					className="text-blue-600 hover:underline"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
