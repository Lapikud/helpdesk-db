"use client";

import { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";

const languages = [
	{ code: "en", label: "English" },
	{ code: "et", label: "Eesti" },
	{ code: "ru", label: "Русский" },
];

const LanguageSwitcher = () => {
  const { i18n } = useTranslation();
  const [open, setOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const toggleDropdown = () => setOpen(!open);

  const changeLanguage = (lng: string) => {
    i18n.changeLanguage(lng);
    setOpen(false);
  };

  // Close dropdown if clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setOpen(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const currentLang = languages.find((l) => l.code === i18n.resolvedLanguage) || languages[0];

  return (
    <div className="relative inline-block text-left" ref={dropdownRef}>
      <button
        onClick={toggleDropdown}
        className="inline-flex justify-center items-center w-24 px-3 py-1 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500"
        aria-haspopup="true"
        aria-expanded={open}
      >
        {currentLang.label}
        <svg
          className="ml-2 h-4 w-4 text-gray-500"
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          aria-hidden="true"
        >
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d={open ? "M5 15l7-7 7 7" : "M19 9l-7 7-7-7"} />
        </svg>
      </button>

      {open && (
        <div className="absolute right-0 mt-1 w-24 bg-white border border-gray-300 rounded-md shadow-lg z-10">
          {languages.map(({ code, label }) => (
            <button
              key={code}
              onClick={() => changeLanguage(code)}
              className={`block w-full px-3 py-1 text-left text-sm ${
                i18n.resolvedLanguage === code
                  ? "bg-blue-600 text-white"
                  : "text-gray-700 hover:bg-blue-50"
              }`}
            >
              {label}
            </button>
          ))}
        </div>
      )}
    </div>
  );
};

export default LanguageSwitcher;
