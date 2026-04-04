"use client";

import { AccountContext } from "@/context/AccountContext";
import { OwnerService } from "@/services/OwnerService";
import { useRouter } from "next/navigation";

import Link from "next/link";
import { use, useContext, useEffect, useMemo, useState } from "react";
import { IOwner } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import { useTranslation } from "react-i18next";

export default function OwnerDetailsClient({
	params,
}: {
	params: Promise<{ id: string }>;
}) {
	const { t: tOwner } = useTranslation("owner");
	const { t: tCommon } = useTranslation("common");
	const { id } = use(params);
	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [data, setData] = useState<IOwner>();
	const [hydrated, setHydrated] = useState(false);
	const ownerService: OwnerService = useMemo(() => new OwnerService(), []);

	if (setAccountInfo) {
		ownerService.injectSetAccountInfo(setAccountInfo);
	}

	const isAdmin = accountInfo?.roles?.includes("admins");
	const isMember = accountInfo?.roles?.includes("members");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;

		if (!accountInfo?.jwt) {
			router.push("/login");
		} else if (!isAdmin) {
			router.push("/owners");
		} else {
			const fetchData = async () => {
				setData((await ownerService.getAsync(id)).data!);
			};
			fetchData();
		}
	}, [hydrated, accountInfo, router, id, isAdmin, ownerService]);

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
					{tOwner("OwnerSingular")}
				</h4>
				<hr className="border-gray-300" />
				<dl className="space-y-2">
					<div className="flex">
						<dt className="w-1/3 font-medium text-gray-700">
							{tOwner("OwnerName")}
						</dt>
						<dd className="w-2/3 text-gray-900">
							{data.ownerName}
						</dd>
					</div>
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
					href={`/owners/edit/${data.id}`}
					className="text-blue-600 hover:underline font-medium"
				>
					{tCommon("EditLink")}
				</Link>
				<span className="text-gray-400"> | </span>
				<Link
					href="/owners"
					className="text-blue-600 hover:underline font-medium"
				>
					{tCommon("BackToListLink")}
				</Link>
			</div>
		</>
	);
}
