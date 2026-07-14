import i18n from "i18next";
import HttpBackend from "i18next-http-backend";
import LanguageDetector from "i18next-browser-languagedetector";
import { initReactI18next } from "react-i18next";

i18n.use(HttpBackend) // loads translations via HTTP
	.use(LanguageDetector) // detects user language
	.use(initReactI18next) // passes i18n to react-i18next
	.init({
		fallbackLng: "en", // fallback language
		ns: ['_assetlistpartial', '_languageselectionpartial', '_layout', '_loginpartial', 'approle', 'appuser', 'appuserrole', 'asset', 'assetviewmodel', 'category', 'categoryassets', 'common', 'createnewasset', 'cupboard', 'cupboardinroom', 'identityerrors', 'location', 'locationassets', 'locationincupboard', 'modelbindingerrors', 'owner', 'ownerassets', 'remove', 'removedassets', 'room', 'userassets', 'validationerrors'],
		defaultNS: 'common',
		debug: process.env.NODE_ENV !== 'production', // helpful for debugging in dev only
		interpolation: {
			escapeValue: false, // React already escapes by default
		},
		// backend: {
		// 	loadPath: "/~maperm/helpdeskDb/locales/{{lng}}/{{ns}}.json", // where to load translations from
		// },
		detection: {
			order: ['localStorage', 'navigator'],
			lookupLocalStorage: 'i18nextLng',
			caches: ['localStorage'],
			checkSupportedLngs: true,
		} as import("i18next-browser-languagedetector").DetectorOptions,
		supportedLngs: ['en', 'et', 'ru'],
		load: 'languageOnly',
	});

export default i18n;
