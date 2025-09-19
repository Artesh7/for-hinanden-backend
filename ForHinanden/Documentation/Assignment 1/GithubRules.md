# Branch Protection: `stage`

For at sikre en stabil udviklingsproces og undgå fejl i `stage`-branchen, er følgende regler opsat:

## ✅ Krav til `stage` branch

1. Alle ændringer til `stage` skal ske via en **Pull Request (PR)**.
2. En PR **skal godkendes af mindst ét andet gruppemedlem**, før den må merges.
3. Det er **ikke tilladt at pushe direkte** til `stage`.
4. *(Valgfrit)*: PR skal passere automatiske tests (CI).

---

## 🔧 Sådan er reglerne konfigureret i GitHub

> Kun admin eller maintainer kan opsætte dette.

1. Gå til repository → **Settings** → **Branches**
2. Klik på **Add rule** under *Branch protection rules*
3. Udfyld følgende:

    - **Branch name pattern**:
      ```
      stage
      ```

4. Marker følgende felter:

    - ✅ `Require a pull request before merging`
    - ✅ `Require approvals` → minimum `1`
    - ✅ `Dismiss stale pull request approvals when new commits are pushed`
    - ✅ *(Valgfri)* `Require status checks to pass before merging`  
      *(Vælg relevante workflows hvis I bruger GitHub Actions)*
    - ✅ *(Valgfri men anbefalet)* `Restrict who can push to matching branches`  
      *(Tilføj kun udvalgte teammedlemmer eller teams)*

5. Klik **Create** for at oprette reglen.

---

## 👥 Anbefaling

Tilføj jeres team som et **GitHub Team** under organisationen, og brug det til at styre hvem der har push/merge-rettigheder til `stage`.

---

## ✅ Eksempel på Pull Request-proces

1. Opret ny feature/bugfix branch:  

2. Lav dine ændringer og push til remote:

3. Opret en Pull Request til `stage` via GitHub

4. Få mindst én godkendelse fra teamet

5. Merge PR, når godkendelse og evt. tests er på plads

---

## 📌 Husk

- Ingen må pushe direkte til `stage`
- Brug branches og PR'er
- Review hinandens kode
