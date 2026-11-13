# 08 — ECS Loan Sanction: Modifications

> Builds on [06-ecs-loan-sanction.md](./06-ecs-loan-sanction.md).
> Page: `Infin8.Coapp.UI.Client/Pages/Products/ECSLoanSanction.razor`
> (routes `/ecs-loan-sanction` and `/loan-sanction`).
>
> **Status:** draft. No code has been changed for this spec yet.
>
> A separate page will cover listing sanctioned loans, viewing them, reprinting the Office Note and
> deleting a sanction. It will be specified on its own once this page is complete (see §6).

---

## Summary

| # | Change | Kind |
|---|---|---|
| 1 | Lock **Member No** once the member has been fetched. Only **Refresh** unlocks it. | Code (client) |
| 2 | The surety overdue table is missing on port 5247 but shows on port 5299. | **No code change.** The local `main` checkout is out of date (§2) |
| 3 | Show the **Office Note** automatically after a successful save, so the user can print it. | Code (client) |
| 4 | Remove the **Sanction List** tab. | Code (client) |
| 5 | Remove the **Delete Sanction** button. | Code (client) |
| 6 | List, view, reprint and delete sanctions go to a separate page. | Out of scope. Future spec |

---

## 1. Lock Member No after fetch

### Problem

The Member No box stays editable after the member's details are fetched. Today it is disabled only in
Modify mode (`disabled="@IsModify"`).

If the user changes Member No by accident before **Save Sanction**, the `onchange` binding calls
`LookupMember()`. That reloads the member, and the loan can then be saved against the wrong member.
A sanction must never be moved silently to a different member.

### Required behaviour

1. The user enters Member No. On change (Tab or Enter), the member's details are fetched as they are
   today.
2. **If the member is found** (`member.Found == true`), the Member No box becomes **disabled**
   (read-only) and stays disabled for the rest of that sanction:
   - while the user works through the tabs;
   - during and after **Save Sanction**.
3. **If the member is not found**, the box stays editable. The user sees the existing lookup message
   and can correct the number.
4. **Refresh** is the only way to unlock the box. It already calls `ResetPage()`, which clears
   `memberNo` and `member`. If the user entered the wrong member, they click **Refresh** and start
   again.

### Implementation notes

- Change the input's disabled condition from `disabled="@IsModify"` to
  `disabled="@(member.Found || busy)"`. `IsModify` is covered too, because `LoadSanction` sets
  `member`.
- No server change. `SaveSanction` already sends `member.Mem_Id`, not the typed text. The risk was
  the re-lookup that an edit triggers, and the lock removes it.
- Optional hint below the box, shown when it is locked: *"Click Refresh to change the member."*

### Acceptance

- Fetch a valid member. The box is greyed out and cannot be typed in or tabbed into.
- Enter an invalid member number. The box stays editable and the error message shows.
- Click **Refresh**. The box is empty and editable, and every tab is cleared.

---

## 2. Surety Member's Loan Overdue missing on port 5247

### What the user sees

| Port | How it is run | What is shown |
|---|---|---|
| 5299 | The PR worktree (`claude/ecs-loan-sanction-surety-b49691`) | The **Surety Details** fieldset in the Loan & Eligibility tab. It has a table with Sl.No, Loan No, Loan Name and Overdue Amount, blocking rows in red, a Total Overdue row and a red message: *"The surety … has overdue in loan(s) …. Loan sanction cannot be saved."* There is also a red page-level banner. |
| 5247 | VS Code, from `/Users/manojg/Documents/ECS` (`main`) | The old layout: a small **"Surety has overdue"** badge and a two-column Loan / Overdue table inside the **Assets** column. There is no Sl.No, no total and no red message. |

### Cause

The code is correct. The local `main` checkout has **not been updated** since PR #7 was merged on
GitHub:

```
local  main        05d6d79   (before the surety work)
origin/main        229dba7   Merge pull request #7
                   f04cd29   ECS Loan Sanction - Surety Details completed
```

`git merge-base --is-ancestor f04cd29 main` returns false. VS Code is therefore building and running
the code from before the merge. Commit `f04cd29` touched 7 files across the UI, Controller, Handler,
Repository and DTO layers, so all of them are stale on 5247, not just the page.

### Resolution (no code change)

In `/Users/manojg/Documents/ECS` (the main checkout used by VS Code):

```bash
git pull origin main
```

Then stop the running app, and rebuild and restart it from VS Code. A rebuild is required because
`Dto` and `BusinessLogic` changed too. A hot reload is not enough.

### Acceptance

Choose a member whose surety has an ECS-loan overdue (for example, Member 002960 with Surety 003141,
K P KARTHIK). On 5247, the Loan & Eligibility tab must look the same as on 5299:
- the Surety Details fieldset and its 4-column table;
- red rows;
- Total Overdue;
- the red message;
- the red banner;
- **Save Sanction** disabled.

---

## 3. Show the Office Note after saving

### Required behaviour

After **Save Sanction** succeeds, the Office Note for the new sanction is shown to the user straight
away, ready to print. The user does not have to find and click **Print Office Note**.

### Design

- **Open it in a modal on the page, not in a new browser tab.** `PrintOfficeNote()` currently calls
  `openPdfInNewTab` (`wwwroot/js/pdfViewer.js`), which uses `window.open`. That call would now run
  after an `await` on the save request, so it is no longer part of the user's click. Browsers then
  treat it as a pop-up and block it, and the user sees the *"Please allow pop-ups"* alert.
- The flow is:
  1. `SaveSanction()` succeeds.
  2. The page sets `sanctionId`.
  3. It calls `GET api/loansanction/OfficeNote/{sanctionId}`. This endpoint already exists and
     returns PDF bytes.
  4. It shows a modal titled **"Office Note — Sanction {id}"**. The PDF is in an `<iframe>` built
     from a Blob URL.
  5. The modal footer has **Print** (calls `print()` on the iframe's content window) and **Close**
     buttons.
- Use the page's existing modal markup (`modal d-block` with a backdrop, the same as the Demand
  modal). Do not add a new modal component.
- Revoke the Blob URL when the modal closes.
- Keep the footer button **Print Office Note** so the user can reopen the note after closing the
  modal. It stays enabled while `sanctionId > 0`.
- **If the Office Note cannot be produced**, the save still stands. Show the success alert followed by
  the Office Note error. Example: the endpoint returns 404 when there is no `Office Note` row in
  `reports_master`.

### After the save

- The sanction is saved and the Member No box stays locked (§1).
- Delete and List are being removed (§4 and §5), so this page no longer edits a saved sanction.
  After a successful save, **disable Save** until **Refresh** is clicked. Without this, a second click
  goes down the Modify path, because `loadedSanctionId` is now set. Change the success message to:
  *"Sanction {id} saved. Click Refresh to start a new sanction."*

### Acceptance

- Save a valid sanction. The Office Note modal opens with the PDF, and no pop-up warning appears.
- **Print** opens the browser print dialog for the note.
- Close the modal. **Print Office Note** reopens it.
- **Save** stays disabled until **Refresh** is clicked.

---

## 4. Remove the Sanction List tab

- Remove `("list", "6. Sanctions")` from `tabs`.
- Remove the `case "list":` block (the filter, the List button and the results table).
- Remove the client-only state and methods that the list used: `listFilter`, `sanctionList` and
  `ListSanctions()`.
- **Keep** `LoadSanction(id)` and the Modify path. The future list page (§6) is expected to open a
  sanction on this page, for example `/ecs-loan-sanction?id=…`. Until that page exists, the Modify
  path is unreachable. This is accepted.
- **Keep** the server endpoints `ListSanctions` and `GetSanction` for the future page.

---

## 5. Remove the Delete Sanction button

- Remove the **Delete Sanction** button from the footer.
- Remove `DeleteSanction()` from the page.
- **Keep** the server endpoint `POST api/loansanction/DeleteSanction/{id}` and the handler logic. The
  future page (§6) will use them.
- Update the alert text in `LoadSanction` so it no longer mentions Delete Sanction.

The footer after the change:

`[Save Sanction]  [Print Office Note]  ……………  [Refresh]`

---

## 6. Out of scope: the sanctioned loans page

A separate prompt and spec will follow once this page is complete. It will cover a page to:
- list sanctioned loans, with filters by Sanction Id, Member No and date range;
- view a sanction;
- reprint the Office Note;
- delete a sanction.

It will reuse the `ListSanctions`, `GetSanction`, `OfficeNote` and `DeleteSanction` endpoints that
§4 and §5 keep.

---

## Files touched (when implemented)

| File | Change |
|---|---|
| `Infin8.Coapp.UI.Client/Pages/Products/ECSLoanSanction.razor` | §1, §3, §4, §5 |
| `Infin8.Coapp.UI.Client/wwwroot/js/pdfViewer.js` | §3, only if a small helper is needed (for example `createPdfUrl` / `printIframe`). It must not change the existing `openPdfInNewTab`, which other pages use |

No change to the server, the database or `reports_master`.

## Verification

There are no automated tests. After `dotnet build Infin8.Coapp.sln`, run the app and work through
the acceptance steps in §1–§5 on the ECS branch (11005). Do this after pulling `main` (§2).
