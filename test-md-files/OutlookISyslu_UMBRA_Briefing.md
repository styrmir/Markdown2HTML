# OutlookISyslu: Add-in Distribution — Briefing for UMBRA Meeting

## Executive Summary

UMBRA has expressed concern about "uploading this to the Microsoft Store." This is an understandable concern — but it is based on a misunderstanding of what is being requested. We are **not** asking to publish the add-in publicly on the Microsoft Store (AppSource). We are asking to use **Centralized Deployment** through the Microsoft 365 Admin Center, which is a private, internal deployment mechanism that the UMBRA administrator has full control over.

---

## The Core Distinction: Two Entirely Different Paths

| | Public Microsoft Store (AppSource) | Centralized Deployment (Admin Center) |
|---|---|---|
| **What is it?** | Public marketplace where anyone can browse and install add-ins | Internal deployment feature in M365 Admin Center |
| **Who can see the add-in?** | Everyone on the internet | Only users within the tenant that the admin has assigned it to |
| **Who controls it?** | Microsoft reviews and publishes | UMBRA admin has full control |
| **How does it work?** | Must pass Microsoft certification process | Admin uploads a manifest XML file |
| **Is it public?** | Yes, public listing | **No** — entirely internal to the tenant |
| **Classification** | Store app | **LOB (Line-of-Business) add-in** |

**We are requesting the second option — Centralized Deployment.**

---

## What Is Centralized Deployment?

Centralized Deployment is a built-in feature of the Microsoft 365 Admin Center that allows a tenant administrator to deploy Office add-ins to users within their organization. The process is:

1. Administrator goes to **Microsoft 365 Admin Center -> Settings -> Integrated Apps**
2. Selects **Upload custom apps**
3. Uploads a manifest file (XML) or provides a URL to the manifest
4. Selects which users or groups the add-in is assigned to
5. Clicks **Deploy**

The manifest file contains only metadata about the add-in: its name, the URL of the hosted web service, the permissions it requests, and its icon. **No executable code is uploaded to Microsoft.** The manifest simply points to a web service that we host on our own infrastructure.

### What Microsoft Says About This

From Microsoft's own documentation:

- Centralized Deployment is **Microsoft's recommended method** for deploying LOB add-ins: the integrated apps portal is described as "the recommended and most feature-rich way for most customers to centrally deploy Office Add-ins."
- It is supported under **Microsoft 365 Government (G3/G5)** licenses.
- **Exchange Online stores the manifest** within the tenant itself — the manifest never leaves the tenant boundary.
- Government cloud tenants that cannot access the public store are explicitly directed to use Centralized Deployment instead.

### What the Manifest File Contains

A manifest file is a simple XML document that describes the add-in:

- Add-in name and description
- URL of the web service where the add-in is hosted (our own server)
- Permissions requested
- Icon and version number

There is **no executable code** in the manifest. All application logic runs on our own hosting infrastructure.

---

## Why Sideloading Is Not an Alternative

| Method | Outlook Classic | New Outlook / Outlook Web |
|---|---|---|
| Sideloading (manual per-user install) | Supported | **Not supported for production** — development only |
| Centralized Deployment (admin deployment) | Supported | **Supported — the only production method** |
| Exchange PowerShell deployment | Supported | Supported |

Microsoft has removed sideloading as a production distribution method in New Outlook. **Centralized Deployment is the only supported method for distributing add-ins in New Outlook** for organizations.

If the District Commissioner's office intends to use New Outlook — which Microsoft is actively migrating all organizations toward — then Centralized Deployment is the only viable path for this add-in.

---

## UMBRA Compliance Assessment

### 1. All Permissions Are Delegated

UMBRA policy requires that permissions should be Delegated. OutlookISyslu requests **only** Delegated permissions:

- Mail.ReadWrite
- offline_access
- openid
- profile
- User.Read

No Application permissions are requested. This is the cleanest category under UMBRA policy and requires no application access policy or security committee review.

### 2. Minimal Permission Scope

`Mail.ReadWrite.Shared` was evaluated and explicitly excluded because the add-in only operates on the user's own mailbox. The current permission set is the minimum required for the standard Office add-in SSO pattern.

### 3. Data Residency — Europe/Iceland

The add-in connects to the Sysla case management system, which is hosted on infrastructure within Iceland. No user data is stored with third-party cloud services outside Europe. The add-in acts as a bridge between the user's Outlook client and the existing Sysla system.

### 4. No Alternative Exists

OutlookISyslu is a purpose-built add-in that integrates Outlook with the Sysla case management system used by the District Commissioner. No existing approved add-in in the Microsoft Store provides this functionality.

### 5. Administrator Retains Full Control

Under Centralized Deployment of a LOB add-in:

- The admin decides which users receive the add-in
- The admin can disable the add-in at any time
- The admin can delete the add-in at any time
- The manifest is stored in Exchange Online within the tenant boundary
- Updates require the admin to manually upload a new manifest file — nothing auto-updates without explicit admin action
- The add-in appears in the admin's Integrated Apps list for full visibility and audit

This is a higher degree of admin control than Store-deployed add-ins, which auto-update when the publisher pushes changes.

---

## What We Are Asking UMBRA to Do

One straightforward administrative action:

**Upload a manifest XML file in the M365 Admin Center and assign it to users.**

This is:

- The same process used for any other internal Office add-in in the tenant
- An entirely internal operation — nothing is published to the internet
- Fully reversible — the admin can remove the add-in at any time
- Logged and visible in the Integrated Apps dashboard
- The method Microsoft explicitly recommends for LOB add-ins
- The method Microsoft directs government cloud tenants to use

---

## Addressing the "Microsoft Store" Concern Directly

The concern appears to stem from the term "Store" or "upload." To be explicit:

| Concern | Reality |
|---|---|
| "We don't want to publish on the Microsoft Store" | We are not asking for this. The add-in will not appear on any public store. |
| "We don't want third parties accessing our tenant" | No third-party access is involved. The admin uploads a manifest that points to our server. |
| "We don't want auto-updates we can't control" | LOB add-ins deployed via Centralized Deployment do not auto-update. The admin must manually upload a new manifest for any change. |
| "We don't want executable code uploaded to Microsoft" | The manifest is metadata only (XML). No code is uploaded. The web service runs on our infrastructure. |
| "Can we use sideloading instead?" | Not for New Outlook. Microsoft has removed sideloading as a production deployment method. Centralized Deployment is the only supported path. |

---

## Reference Links (Microsoft Documentation)

- [Deploy Office Add-ins in the admin center](https://learn.microsoft.com/en-us/microsoft-365/admin/manage/manage-deployment-of-add-ins)
- [Centralized Deployment requirements](https://learn.microsoft.com/en-us/microsoft-365/admin/manage/centralized-deployment-of-add-ins)
- [Centralized Deployment FAQ](https://learn.microsoft.com/en-us/microsoft-365/admin/manage/centralized-deployment-faq)
- [Government cloud add-in guidance](https://learn.microsoft.com/en-us/office/dev/add-ins/publish/government-cloud-guidance)
- [Deploy and publish Office Add-ins (overview)](https://learn.microsoft.com/en-us/office/dev/add-ins/publish/publish)

---

## Proposed Next Steps

1. UMBRA admin opens M365 Admin Center -> Settings -> Integrated Apps
2. Selects "Upload custom apps" -> "Office Add-in"
3. Uploads the manifest file that we provide
4. Assigns the add-in to District Commissioner users
5. Sends us the Application ID URI and Tenant ID (securely, e.g. via OneTimeSecret)
6. We complete the backend configuration

All of this is an **internal operation** within the UMBRA tenant. Nothing is published to any public store or marketplace.
