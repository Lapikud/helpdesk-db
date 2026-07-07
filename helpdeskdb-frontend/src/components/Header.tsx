"use client";
import LanguageSwitcher from "@/components/LanguageSwitcher";
import { AccountContext } from "../context/AccountContext";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useContext, useState, useRef, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { AccountService } from "@/services/AccountService";

export default function Header() {
	const { t: tLayout } = useTranslation("_layout");
	const { t: tCommon } = useTranslation("common");
	const { t: tLoginPartial } = useTranslation("_loginpartial");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const router = useRouter();
	const [isUserDropdownOpen, setIsUserDropdownOpen] = useState(false);
	const userDropdownRef = useRef<HTMLDivElement>(null);

	const [isAdminDropdownOpen, setIsAdminDropdownOpen] = useState(false);
	const adminDropdownRef = useRef<HTMLDivElement>(null);

	const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

	const isAdmin = accountInfo?.roles?.includes("admins");

	useEffect(() => {
		function handleClickOutside(event: MouseEvent) {
			if (
				userDropdownRef.current &&
				!userDropdownRef.current.contains(event.target as Node)
			) {
				setIsUserDropdownOpen(false);
			}
			if (
				adminDropdownRef.current &&
				!adminDropdownRef.current.contains(event.target as Node)
			) {
				setIsAdminDropdownOpen(false);
			}
		}
		document.addEventListener("mousedown", handleClickOutside);
		return () => {
			document.removeEventListener("mousedown", handleClickOutside);
		};
	}, [userDropdownRef, adminDropdownRef]);

	const closeMobileMenu = () => setIsMobileMenuOpen(false);

	const handleLogout = async () => {
		const accountService = new AccountService();
		await accountService.logoutAsync();
		setAccountInfo!({});
		router.push("/login");
		closeMobileMenu();
	};

	return (
		<nav className="bg-white border-b shadow mb-3">
			{/* Desktop + tablet bar */}
			<div className="px-6 sm:px-14 py-3 flex items-center justify-between">
				{/* Left side: Brand + Main nav (hidden on mobile) */}
				<div className="flex items-center space-x-6">
					<Link
						href="/"
						className="text-xl font-semibold text-gray-800"
					>
						{tLayout("AppName")}
					</Link>

					<Link
						href="/"
						className="hidden md:inline text-gray-700 hover:text-blue-500"
					>
						{tCommon("Home")}
					</Link>

					{/* User Dropdown — desktop only */}
					{accountInfo?.id && (
						<div className="relative hidden md:block" ref={userDropdownRef}>
							<button
								onClick={() =>
									setIsUserDropdownOpen(!isUserDropdownOpen)
								}
								className="text-gray-700 hover:text-blue-500 flex items-center gap-1"
							>
								{tLayout("User")}
								<svg
									className="w-4 h-4"
									fill="none"
									stroke="currentColor"
									viewBox="0 0 24 24"
								>
									<path
										strokeLinecap="round"
										strokeLinejoin="round"
										strokeWidth={2}
										d="M19 9l-7 7-7-7"
									/>
								</svg>
							</button>
							{isUserDropdownOpen && (
								<div className="absolute mt-2 w-48 bg-white border border-gray-200 rounded shadow z-50">
									<Link
										href="/assetReservations"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsUserDropdownOpen(false)
										}
									>
										{tLayout("AssetReservations")}
									</Link>
									<Link
										href="/categories"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsUserDropdownOpen(false)
										}
									>
										{tLayout("Categories")}
									</Link>
									<Link
										href="/owners"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsUserDropdownOpen(false)
										}
									>
										{tLayout("Owners")}
									</Link>
									<Link
										href="/removedAssets"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsUserDropdownOpen(false)
										}
									>
										{tLayout("RemovedAssets")}
									</Link>
								</div>
							)}
						</div>
					)}
					{/* Admin Dropdown — desktop only */}
					{accountInfo?.id && isAdmin && (
						<div className="relative hidden md:block" ref={adminDropdownRef}>
							<button
								onClick={() =>
									setIsAdminDropdownOpen(!isAdminDropdownOpen)
								}
								className="text-gray-700 hover:text-blue-500 flex items-center gap-1"
							>
								{tLayout("Admin")}
								<svg
									className="w-4 h-4"
									fill="none"
									stroke="currentColor"
									viewBox="0 0 24 24"
								>
									<path
										strokeLinecap="round"
										strokeLinejoin="round"
										strokeWidth={2}
										d="M19 9l-7 7-7-7"
									/>
								</svg>
							</button>
							{isAdminDropdownOpen && (
								<div className="absolute mt-2 w-48 bg-white border border-gray-200 rounded shadow z-50">
									<Link
										href="/dbassets"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("Assets")}
									</Link>
									<Link
										href="/assetReservations"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("AssetReservations")}
									</Link>
									<Link
										href="/categories"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("Categories")}
									</Link>
									<Link
										href="/categoryAssets"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("CategoryAssets")}
									</Link>
									<Link
										href="/cupboards"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("Cupboards")}
									</Link>
									<Link
										href="/cupboardsInRooms"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("CupboardsInRooms")}
									</Link>
									<Link
										href="/locations"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("Locations")}
									</Link>
									<Link
										href="/locationAssets"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("LocationAssets")}
									</Link>
									<Link
										href="/owners"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("Owners")}
									</Link>
									<Link
										href="/ownerAssets"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("OwnerAssets")}
									</Link>
									<Link
										href="/rooms"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("Rooms")}
									</Link>
									<Link
										href="/removedAssets"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("RemovedAssets")}
									</Link>
									<hr className="my-1 border-gray-200" />
									<Link
										href="/users"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("Users")}
									</Link>
									<Link
										href="/roles"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("Roles")}
									</Link>
									<Link
										href="/refreshTokens"
										className="block px-4 py-2 text-sm hover:bg-gray-100 text-gray-700"
										onClick={() =>
											setIsAdminDropdownOpen(false)
										}
									>
										{tLayout("RefreshTokens")}
									</Link>
								</div>
							)}
						</div>
					)}
				</div>

				{/* Right side — desktop */}
				<div className="hidden md:flex items-center space-x-6">
					<LanguageSwitcher />
					{accountInfo?.id && (
						<div className="flex items-center space-x-4">
							<span>
								{tLoginPartial("Greeting")}, {accountInfo.name}
							</span>
							<a
								className="text-gray-700 hover:text-blue-500"
								href="#"
								onClick={handleLogout}
							>
								{tCommon("Logout")}
							</a>
						</div>
					)}
					{!accountInfo?.id && (
						<Link
							href="/login"
							className="text-gray-700 hover:text-blue-500"
						>
							{tCommon("LoginLink")}
						</Link>
					)}
				</div>

				{/* Hamburger button — mobile only */}
				<button
					className="md:hidden p-2 rounded text-gray-700 hover:bg-gray-100"
					onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
					aria-label="Toggle menu"
				>
					{isMobileMenuOpen ? (
						<svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
						</svg>
					) : (
						<svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
						</svg>
					)}
				</button>
			</div>

			{/* Mobile menu drawer */}
			{isMobileMenuOpen && (
				<div className="md:hidden border-t bg-white px-4 py-3 flex flex-col gap-1">
					<Link href="/" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100" onClick={closeMobileMenu}>
						{tCommon("Home")}
					</Link>

					{accountInfo?.id && (
						<>
							<div className="px-3 py-1 text-xs font-semibold text-gray-400 uppercase tracking-wider mt-2">
								{tLayout("User")}
							</div>
							<Link href="/assetReservations" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("AssetReservations")}
							</Link>
							<Link href="/categories" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("Categories")}
							</Link>
							<Link href="/owners" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("Owners")}
							</Link>
							<Link href="/removedAssets" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("RemovedAssets")}
							</Link>
						</>
					)}

					{accountInfo?.id && isAdmin && (
						<>
							<div className="px-3 py-1 text-xs font-semibold text-gray-400 uppercase tracking-wider mt-2">
								{tLayout("Admin")}
							</div>
							<Link href="/dbassets" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("Assets")}
							</Link>
							<Link href="/assetReservations" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("AssetReservations")}
							</Link>
							<Link href="/categories" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("Categories")}
							</Link>
							<Link href="/categoryAssets" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("CategoryAssets")}
							</Link>
							<Link href="/cupboards" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("Cupboards")}
							</Link>
							<Link href="/cupboardsInRooms" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("CupboardsInRooms")}
							</Link>
							<Link href="/locations" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("Locations")}
							</Link>
							<Link href="/locationAssets" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("LocationAssets")}
							</Link>
							<Link href="/owners" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("Owners")}
							</Link>
							<Link href="/ownerAssets" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("OwnerAssets")}
							</Link>
							<Link href="/rooms" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("Rooms")}
							</Link>
							<Link href="/removedAssets" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("RemovedAssets")}
							</Link>
							<hr className="my-1 border-gray-200" />
							<Link href="/users" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("Users")}
							</Link>
							<Link href="/roles" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("Roles")}
							</Link>
							<Link href="/refreshTokens" className="block px-3 py-2 rounded text-gray-700 hover:bg-gray-100 pl-5" onClick={closeMobileMenu}>
								{tLayout("RefreshTokens")}
							</Link>
						</>
					)}

					<hr className="my-2 border-gray-200" />
					<div className="flex items-center justify-between px-3 py-2">
						<LanguageSwitcher />
						{accountInfo?.id ? (
							<div className="flex items-center gap-3">
								<span className="text-sm text-gray-600">{accountInfo.name}</span>
								<a className="text-sm text-gray-700 hover:text-blue-500" href="#" onClick={handleLogout}>
									{tCommon("Logout")}
								</a>
							</div>
						) : (
							<Link href="/login" className="text-gray-700 hover:text-blue-500" onClick={closeMobileMenu}>
								{tCommon("LoginLink")}
							</Link>
						)}
					</div>
				</div>
			)}
		</nav>
	);
}
