import { useTranslation } from "react-i18next";

export default function Footer() {
	const { t } = useTranslation("_layout");
	return (
		<footer className="fixed bottom-0 left-0 w-full border-t text-gray-500 text-sm bg-white">
			<div className="container mx-auto px-4 py-4 text-center sm:text-left">
				&copy; 2025 - {t("AppName")}
			</div>
		</footer>
	);
}
