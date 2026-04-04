"use client";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { CupboardsInRoomsService } from "@/services/CupboardsInRoomsService";
import { CupboardService } from "@/services/CupboardService";
import { RoomService } from "@/services/RoomService";
import { ICupboard, IRoom, ICupboardInRoom } from "@/types/domain/DomainTypes";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";

export default function CupboardsInRoomsEdit({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tCupboardsInRooms } = useTranslation("cupboardinroom");
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [isLoading, setIsLoading] = useState(true);
	const [errorMessage, setErrorMessage] = useState("");

	const [cupboardsData, setCupboardsData] = useState<ICupboard[]>([]);
	const [categoriesData, setRoomsData] = useState<IRoom[]>([]);
	const [updateComment, setUpdateComment] = useState("");
	const [commentError, setCommentError] = useState<string | null>(null);

	const cupboardsInRoomsService: CupboardsInRoomsService = useMemo(
		() => new CupboardsInRoomsService(),
		[]
	);
	const cupboardService: CupboardService = useMemo(() => new CupboardService(), []);
	const roomService: RoomService = useMemo(
		() => new RoomService(),
		[]
	);
	if (setAccountInfo) {
		cupboardsInRoomsService.injectSetAccountInfo(setAccountInfo);
		cupboardService.injectSetAccountInfo(setAccountInfo);
		roomService.injectSetAccountInfo(setAccountInfo);
	}

	const isAdmin = accountInfo?.roles?.includes("admins");

	const validateComment = (comment: string) => {
		if (!comment || comment.trim().length === 0) {
			setCommentError(
				tValidation("Required", { field: tCommon("Comment") })
			);
			return false;
		}
		if (comment.length < 2) {
			setCommentError(
				tValidation("MinLenghtValidationError", {
					field: tCommon("Comment"),
					min: 2,
				})
			);
			return false;
		}
		if (comment.length > 255) {
			setCommentError(
				tValidation("MaxLengthValidationError", {
					field: tCommon("Comment"),
					max: 255,
				})
			);
			return false;
		}
		setCommentError(null);
		return true;
	};

	const {
		register,
		handleSubmit,
		formState: { errors },
		reset,
		setValue,
	} = useForm<ICupboardInRoom>();

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

				// get this RoomCupboard
				const thisCupboardsInRooms = await cupboardsInRoomsService.getAsync(
					id
				);

				if (thisCupboardsInRooms.errors || !thisCupboardsInRooms.data) {
					setErrorMessage(
						thisCupboardsInRooms.errors?.join(", ") ||
							"Failed to load room cupboards"
					);
					return;
				}

				// get all cupboards
				const cupboardsResult = await cupboardService.getAllAsync();
				if (cupboardsResult.errors) {
					console.log(cupboardsResult.errors);
					return;
				}

				// set cupboardsData to this current room
				setCupboardsData(
					cupboardsResult.data!.filter(
						(u) => u.id === thisCupboardsInRooms.data!.cupboardId
					)
				);

				// get all categoreis
				const categoriesResult = await roomService.getAllAsync();
				if (categoriesResult.errors) {
					console.log(categoriesResult.errors);
					return;
				}

				setRoomsData(categoriesResult.data!);
				setUpdateComment(thisCupboardsInRooms.data.comment!);

				reset({
					roomId: thisCupboardsInRooms.data.roomId,
					cupboardId: thisCupboardsInRooms.data.cupboardId,
					comment: thisCupboardsInRooms.data.comment
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
		cupboardService,
		roomService,
	]);

	const onSubmit: SubmitHandler<ICupboardInRoom> = async (
		data: ICupboardInRoom
	) => {
		setErrorMessage("Loading...");
		try {
			const result = await cupboardsInRoomsService.updateAsync({
				id: id,
				cupboardId: data.cupboardId,
				roomId: data.roomId,
				comment: updateComment,
			});
			console.log("edit result", result);

			if (result.errors && result.errors.length > 0) {
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
				{tCupboardsInRooms("CupboardsInRooms")}
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
							{tCupboardsInRooms("Cupboard")}
						</label>
						<select
							id="cupboardId"
							{...register("cupboardId", {
								required: tValidation("Required", {
									field: tCupboardsInRooms("Cupboard"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectAn")} {tCupboardsInRooms("Cupboard")}
							</option>
							{cupboardsData.map((cupboard) => (
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
							htmlFor="As"
						>
							{tCupboardsInRooms("Room")}
						</label>
						<select
							id="roomId"
							{...register("roomId", {
								required: tValidation("Required", {
									field: tCupboardsInRooms("Room"),
								}),
							})}
							className="w-full p-2 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
						>
							<option value="">
								{tCommon("SelectA")}{" "}
								{tCupboardsInRooms("Room")}
							</option>
							{categoriesData.map((room) => (
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
							value={updateComment}
							onChange={(e) => setUpdateComment(e.target.value)}
							onBlur={() => validateComment(updateComment)}
							placeholder={tCommon("CommentPrompt")}
							maxLength={255}
						/>
						{commentError && (
							<span className="text-red-600 text-sm">
								{commentError}
							</span>
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
					href="/cupboardsInRooms"
					className="text-blue-600 hover:underline"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
