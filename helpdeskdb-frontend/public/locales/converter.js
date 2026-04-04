// convert-resx.js
const fs = require("fs");
const path = require("path");
const xml2js = require("xml2js");

const parser = new xml2js.Parser();
const inputDir =
	"C:\\Users\\Mark\\Desktop\\Rider\\icd0024-24-25-s\\project\\HelpdeskDb\\Base.Resources";
const outputDir = "./";

// Matches: Category.resx (default to "en"), Category.et.resx, Category.ru.resx
const resxRegex = /^(?<name>\w+)(\.(?<lang>\w{2}))?\.resx$/;

fs.readdirSync(inputDir).forEach((file) => {
	const match = file.match(resxRegex);
	if (!match) return;

	const name = match.groups.name;
	const lang = match.groups.lang || "en"; // default to English if no lang

	const filePath = path.join(inputDir, file);

	fs.readFile(filePath, (err, data) => {
		if (err) throw err;

		parser.parseString(data, (err, result) => {
			if (err) throw err;

			const entries = {};
			if (!result.root?.data) return;

			result.root.data.forEach((item) => {
				const key = item.$.name;
				const value = item.value?.[0] || "";
				entries[key] = value;
			});

			const langDir = path.join(outputDir, lang);
			fs.mkdirSync(langDir, { recursive: true });

			const outputPath = path.join(langDir, `${name.toLowerCase()}.json`);
			fs.writeFileSync(outputPath, JSON.stringify(entries, null, 2));
			console.log(`✅ ${file} → ${outputPath}`);
		});
	});
});
