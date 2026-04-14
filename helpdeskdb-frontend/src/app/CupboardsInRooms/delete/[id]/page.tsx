"use client";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { RoomService } from "@/services/RoomService";
import { CupboardsInRoomsService } from "@/services/CupboardsInRoomsService";
import { CupboardService } from "@/services/CupboardService";
import { useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";
import { ICupboardInRoomWithNames } from "@/types/domain/DomainTypes";

export default function CupboardsInRoomsDelete({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tCupboardsInRooms } = useTranslation("cupboardinroom");
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [hydrated, setHydrated] = useState(false);
	const [errorMessage, setErrorMessage] = useState("");
	const [data, setData] = useState<ICupboardInRoomWithNames>();
	const cupboardsInRoomsService: CupboardsInRoomsService = useMemo(
		() => new CupboardsInRoomsService(),
		[]
	);
	const roomService: RoomService = useMemo(() => new RoomService(), []);
	const cupboardService: CupboardService = useMemo(() => new CupboardService(), []);
	if (setAccountInfo) {
		cupboardsInRoomsService.injectSetAccountInfo(setAccountInfo);
		roomService.injectSetAccountInfo(setAccountInfo);
		cupboardService.injectSetAccountInfo(setAccountInfo);
	}

	const isAdmin = accountInfo?.roles?.includes("admins");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

  		if (!isAdmin) {
			router.push("/");
		}
		const fetchData = async () => {
			try {
				const result = await cupboardsInRoomsService.getAsync(id);
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				const cupboardsInRooms = result.data!;

				const roomResult = (await roomService.getAsync(cupboardsInRooms.roomId));
				let roomName;
				if (roomResult.errors) {
					console.log(roomResult.errors);
					return;
				} else {
					roomName = roomResult.data?.roomName!;
				}

				const cupboardResult = (await cupboardService.getAsync(cupboardsInRooms.cupboardId));
				let codeName;
				if (cupboardResult.errors) {
					console.log(cupboardResult.errors);
					return;
				} else {
					codeName = cupboardResult.data?.codeName!;
				}

				setData({...cupboardsInRooms, roomName, codeName});
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};
		fetchData();
	}, [hydrated, router, id, isAdmin, cupboardsInRoomsService, cupboardService, roomService]);

	const deleteConfirmed = async () => {
		try {
			const result = await cupboardsInRoomsService.deleteAsync(id);

			console.log("delete result", result);

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
					{tCupboardsInRooms("CupboardsInRooms")}
				</h4>
				<hr className="border-gray-300" />
				{errorMessage.length > 0 && (
					<p className="text-red-600">{errorMessage}</p>
				)}
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tCupboardsInRooms("Room")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.roomName}</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tCupboardsInRooms("Cupboard")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.codeName}</dd>
					</div>
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tCommon("Comment")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.comment}</dd>
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
						href="/cupboardsInRooms"
						className="text-blue-600 hover:underline font-medium"
					>
						{tCommon("BackToListLink")}
					</Link>
				</div>
			</div>
		</>
	);
}
