# Requirements — Course Platform

Source of truth for business rules. Original Arabic version: `../../Requirements Arabic.txt`.
If code and this file disagree, this file wins — or ask the user to change it.

## 1. Overview

- Backend-only **ASP.NET Core Web API** on **.NET 11**.
- Manages courses, instructors, students, enrollments, invoices, payments, refunds and notifications.

## 2. Actors and roles

| Role | Who | Can do |
|---|---|---|
| `Student` | Regular registered user | Browse published courses, enroll, cancel, pay, request refunds, see own enrollments and invoices |
| `Instructor` | The "Admin" of the system | Create courses, set prices and capacity, publish, see students enrolled in **their own** courses only |

- Authentication: **ASP.NET Core Identity**; the **email** is the login name.
- Each student has a profile.

## 3. Modules and business rules

Each rule has an ID. Code comments and tests reference these IDs (e.g. `// ENR-02`).

### 3.1 Users

- **USR-01** Users register and log in with email + password (Identity bearer token endpoints).
- **USR-02** Email is unique.
- **USR-03** Roles: `Student`, `Instructor`. New registrations are `Student`.
- **USR-04** A default instructor account is seeded for development.
- **USR-05** A student has a profile (full name, email, phone number, registration date).
- **USR-06** An existing instructor can promote a user to `Instructor`. There is no separate Admin role.

### 3.2 Courses

- **CRS-01** A course has: title, description, instructor, price, status, capacity.
- **CRS-02** Status is `Draft` or `Published`. New courses start as `Draft`.
- **CRS-03** A course **cannot be published without a price and an instructor**.
- **CRS-04** Capacity is a positive integer = the maximum number of confirmed + pending enrollments.
- **CRS-05** Only the owning instructor can edit, price or publish a course.
- **CRS-06** Students see only `Published` courses.

### 3.3 Enrollment

- **ENR-01** An enrollment's states: `Pending` → `Confirmed` or `Cancelled`. No other transitions.
- **ENR-02** Seats taken (`Pending` + `Confirmed`) **never exceed** the course capacity, including under concurrent requests.
- **ENR-03** When the course is full, the student joins the **waitlist** (first-in, first-out).
- **ENR-04** When an enrollment is cancelled, the **first student on the waitlist** takes the seat (becomes a `Pending` enrollment).
- **ENR-05** A student can't hold more than one active enrollment or waitlist entry for the same course.
- **ENR-06** Only `Published` courses accept enrollments.
- **ENR-07** An instructor can list students enrolled in their own courses.

### 3.4 Billing

- **BIL-01** A `Pending` enrollment creates an **invoice** for the course price.
- **BIL-02** A paid invoice is **immutable** (no changes to amount or lines).
- **BIL-03** A payment attempt can **succeed** or **fail**.
- **BIL-04** A payment that is still pending after **30 minutes** expires.
- **BIL-05** A refund is allowed only within **14 days** of payment.
- **BIL-06** A successful payment confirms the enrollment.
- **BIL-07** Payments go through an internal **fake payment gateway** (configurable to succeed/fail). No real provider.

### 3.5 Notifications

- **NTF-01** When an enrollment becomes `Confirmed`, a message is sent to the student.
- **NTF-02** Every sent message is stored in a **message log** (recipient, subject, body, sent time, related enrollment).
- **NTF-03** Sending uses a fake sender (no real email).

## 4. Open questions

Decide these with the user before the phase that needs them:

- What happens to a `Pending` enrollment when its payment fails or expires? Proposed: it is cancelled and the seat goes to the waitlist (ENR-04).
- Does a refund cancel the enrollment? Proposed: yes.
- Is there a time limit for a promoted waitlist student to pay? Proposed: the same 30-minute payment rule (BIL-04).
- Currency: proposed single currency, `decimal(18,2)`.
