# Community Resource Finder — Initial Requirements

## 1. Purpose

The application will help people in need, including people experiencing poverty or homelessness, find useful community resources.

## 2. Users

The application will support two initial user types:

- **Guest:** Uses the application without creating or signing into an account.
- **Registered user:** Has an account and can submit a resource for verification.

Additional roles, such as a verifier or administrator, have not yet been defined.

## 3. Functional Requirements

### 3.1 Access and accounts

- A person must be able to access the application without an account.
- A guest must be able to browse available resources.
- A guest must be able to view resources on a map.
- A person must have an account and be signed in before submitting a new resource.

### 3.2 Resource directory

- The application must display a list of resources.
- Each resource must contain enough information for a person to understand what help is offered and where or how to obtain it.
- The application must provide sorting options for the resource list.
- The exact resource fields and sorting options are still to be determined.

### 3.3 Resource map

- The application must display resources on a map.
- A person must be able to identify a resource and its location from the map.
- The relationship between the list and map views is still to be determined.

### 3.4 Resource submission and verification

- Only a signed-in registered user may submit a resource.
- A submitted resource must be verified.
- The application must distinguish unverified submissions from verified resources.
- The verification process, authorized verifier role, and rules governing when a resource becomes publicly visible are still to be determined.

### 3.5 Analytics

- The application is expected to include analytics in a future phase.
- Analytics measures, intended users, reporting views, and privacy requirements are not yet defined.
- Analytics are not part of the initial implementation scope unless added later.

## 4. Access Summary

| Capability | Guest | Registered user |
| --- | --- | --- |
| Browse resources | Yes | Yes |
| View the resource map | Yes | Yes |
| Sort resources | Yes | Yes |
| Submit a resource | No | Yes |
| Verify a resource | Not defined | Not defined |
| View analytics | Not defined | Not defined |

## 5. Initial Scope

The initial application scope includes:

- Account-optional access
- A resource list
- A resource map
- Resource sorting
- User accounts and sign-in for resource submission
- A verification state for submitted resources

Future scope includes analytics and any capabilities assigned to verifier or administrator roles.

## 6. Open Questions

The following decisions are required before implementation planning:

1. What information must each resource contain (for example, name, category, address, phone number, hours, eligibility, and website)?
2. Which sorting and filtering options are required?
3. Who verifies submitted resources, and what does verification involve?
4. Are unverified resources hidden from the public, or displayed with an unverified label?
5. Can registered users edit resources after submission or verification?
6. Which account information is required?
7. Should the application use the device's location to show nearby resources?
8. What analytics should be collected, who may view them, and how will user privacy be protected?
9. Is the first release intended for Windows, Android, iOS, or multiple platforms?

## 7. Constraints and Quality Considerations

Because the intended audience may include vulnerable users, later design work should address:

- Accessibility and simple navigation
- Privacy and collection of only necessary personal information
- Clear indication of whether resource information is verified and current
- Resilience when connectivity is limited
- Protection against false, harmful, or duplicate resource submissions

These are design considerations, not yet confirmed acceptance criteria.
