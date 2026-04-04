"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { CupboardsInRoomsService } from "@/services/CupboardsInRoomsService";
import { CupboardService } from "@/services/CupboardService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { ICupboardInRoom, ICupboardInRoomWithNames } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import { RoomService } from "@/services/RoomService";

export default function CupboardsInRooms() {
	const { t: tCupboardInRoom } = useTranslation("cupboardInRoom");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const cupboardsInRoomsService: CupboardsInRoomsService = useMemo(
		() => new CupboardsInRoomsService(),
		[]
	);
	const cupboardService: CupboardService = useMemo(
		() => new CupboardService(),
		[]
	);
	const roomService: RoomService = useMemo(
			() => new RoomService(),
			[]
		);
	if (setAccountInfo) {
		cupboardsInRoomsService.injectSetAccountInfo(setAccountInfo);
		cupboardService.injectSetAccountInfo(setAccountInfo);
		roomService.injectSetAccountInfo(setAccountInfo);
	}
	const router = useRouter();
	const [data, setData] = useState<ICupboardInRoomWithNames[]>([]);
	const [hydrated, setHydrated] = useState(false);

	const isAdmin = accountInfo?.roles?.includes("admins");

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
				const result = await cupboardsInRoomsService.getAllAsync();
				if (result.errors) {
					console.log(result.errors);
					return;
				}
				const cupboardsInRoomsWithNames = await Promise.all(
                    result.data!.map(async (cupboardsInRoom) => {
                        const room = await roomService.getAsync(cupboardsInRoom.roomId);
                        const cupboard = await cupboardService.getAsync(cupboardsInRoom.cupboardId);
                        const roomName = room.data?.roomName ?? cupboardsInRoom.roomId;
                        const codeName = cupboard.data?.codeName ?? cupboardsInRoom.cupboardId;
                        return { ...cupboardsInRoom, roomName, codeName };
                    })
                );
                setData(cupboardsInRoomsWithNames);
			} catch (error) {
				console.error("Error fetching data:", error);
			}
		};

		fetchData();
	}, [hydrated, accountInfo, router, cupboardsInRoomsService]);

	if (!hydrated) {
		return <Spinner className="h-64" />;
	}

	return (
		<>
			<h1 className="text-3xl font-semibold mb-4">
				{tCupboardInRoom("CupboardsInRooms")}
			</h1>
			{(isAdmin) && (
				<p className="mb-4">
					<Link
						href="/cupboardsInRooms/create"
						className="inline-block bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition"
					>
						{tCommon("CreateNewLink")}
					</Link>
				</p>
			)}

			<div className="w-full max-w-7xl overflow-x-auto shadow rounded-lg">
				<table className="w-full table-auto bg-white border border-gray-200 text-left">
					<thead className="bg-gray-100">
						<tr>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tCupboardInRoom("Room")}
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tCupboardInRoom("Cupboard")}
							</th>
							<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
								{tCommon("Comment")}
							</th>

							{(isAdmin) && (
								<th className="px-6 py-3 text-sm font-semibold text-gray-700 border-b whitespace-nowrap">
									{tCommon("Actions")}
								</th>
							)}
						</tr>
					</thead>
					<tbody>
						{data.map((item) => (
							<tr key={item.id} className="hover:bg-gray-50">
								<td className="px-6 py-4 border-b">
									{item.roomName}
								</td>
								<td className="px-6 py-4 border-b">
									{item.codeName}
								</td>
								<td className="px-6 py-4 border-b">
									{item.comment}
								</td>

								{(isAdmin) && (
									<td className="px-6 py-4 border-b text-blue-600 space-x-2">
										<Link
											href={`/cupboardsInRooms/edit/${item.id}`}
											className="hover:underline"
										>
											{tCommon("EditLink")}
										</Link>
										<span className="text-gray-400">|</span>
										<Link
											href={`/cupboardsInRooms/details/${item.id}`}
											className="hover:underline"
										>
											{tCommon("DetailsLink")}
										</Link>
										<span className="text-gray-400">|</span>
										<Link
											href={`/cupboardsInRooms/delete/${item.id}`}
											className="hover:underline"
										>
											{tCommon("DeleteLink")}
										</Link>
									</td>
								)}
							</tr>
						))}
					</tbody>
				</table>
			</div>
		</>
	);
}
