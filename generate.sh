#!/bin/bash
## install weasyprint + pandoc before calling this script

gsm_numbers=(
  "0470123456"
  "0498765432"
  "0487654321"
  "0466123456"
  "0477123456"
  "0491234567"
  "0489123456"
  "0467123456"
  "0478123456"
  "0494123456"
)

mkdir -p justificatifs

for gsm in "${gsm_numbers[@]}"
do
  htmlfile="justificatifs/justificativeAbsence_${gsm}.html"
  pdffile="justificatifs/justificativeAbsence_${gsm}.pdf"

  cat > temp.md << EOF
---
title: "Certificat d'Absence"
lang: fr
---

Certificat d'Absence

Numéro GSM: ${gsm}

Ce document atteste l'absence pour la période indiquée.

Date: $(date +%d/%m/%Y)
EOF

  pandoc temp.md -o "$htmlfile"
  weasyprint "$htmlfile" "$pdffile"
  rm "$htmlfile"

  echo "Créé : $pdffile"
done

rm temp.md
