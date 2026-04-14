"use client";

import { AccountContext } from "@/context/AccountContext";
import { RoomService } from "@/services/RoomService";
import { CupboardsInRoomsService } from "@/services/CupboardsInRoomsService";
import { CupboardService } from "@/services/CupboardService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { ICupboardInRoomWithNames } from "@/types/domain/DomainTypes";
import { useTranslation } from "react-i18next";
import Spinner from "@/components/LoadingSpinner";

export default function CupboardsInRoomsDetails({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tCupboardInRoom } = useTranslation("cupboardinroom");
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [data, setData] = useState<ICupboardInRoomWithNames>();
	const [hydrated, setHydrated] = useState(false);
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
			return;
		} else {
			const fetchData = async () => {
				try {
					const result = await cupboardsInRoomsService.getAsync(id);
					if (result.errors) {
						console.log(result.errors);
						return;
					}
					const cupboardRoom = result.data!;

					const roomResult = await roomService.getAsync(
						cupboardRoom.roomId
					);
					let roomName;
					if (roomResult.errors) {
						console.log(roomResult.errors);
						return;
					} else {
						roomName = roomResult.data?.roomName!;
					}

					const cupboardResult = await cupboardService.getAsync(
						cupboardRoom.cupboardId
					);
					let codeName;
					if (cupboardResult.errors) {
						console.log(cupboardResult.errors);
						return;
					} else {
						codeName = cupboardResult.data?.codeName!;
					}

					setData({ ...cupboardRoom, roomName, codeName });
				} catch (error) {
					console.error("Error fetching data:", error);
				}
			};
			fetchData();
		}
	}, [hydrated, router, id, cupboardsInRoomsService, roomService, cupboardService, isAdmin]);

	if (!hydrated || !data) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-2">
				{tCommon("DetailsTitle")}
			</h1>

			<div className="bg-white p-6 rounded-lg shadow-md max-w-xl mx-auto space-y-4">
				<h4 className="text-xl font-medium text-gray-800">
					{tCupboardInRoom("CupboardsInRooms")}
				</h4>
				<hr className="border-gray-300" />
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tCupboardInRoom("Room")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.roomName}</dd>
					</div>
				</dl>
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tCupboardInRoom("Cupboard")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.codeName}</dd>
					</div>
				</dl>
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tCommon("Comment")}
						</dt>
						<dd className="w-2/3 text-gray-900">{data.comment}</dd>
					</div>
				</dl>
			</div>
			<div>
				<Link
					href="/cupboardsInRooms"
					className="text-blue-600 hover:underline font-medium"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
