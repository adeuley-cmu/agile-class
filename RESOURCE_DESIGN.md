# Community Resource Design

## Resource fields

| Field | Type | Required | Purpose |
| --- | --- | --- | --- |
| `Id` | UUID | Yes | Uniquely identifies the resource. |
| `Name` | Text | Yes | Public name of the organization or service. |
| `Category` | Text | Yes | Groups similar services for later filtering and sorting. |
| `Locations` | List of locations | Yes | Holds one or more physical addresses for the resource. |
| `Description` | Text | No | Explains what help the resource provides. |
| `Phone` | Text | No | Contact telephone number. |
| `Website` | Text | No | Public website address. |
| `IsVerified` | Boolean | Yes | Indicates whether the resource has completed verification. |
| `CreatedAt` | Date/time | Yes | Records when the resource was submitted. |

## Location fields

| Field | Type | Required | Purpose |
| --- | --- | --- | --- |
| `Id` | UUID | Yes | Uniquely identifies a location. |
| `FullAddress` | Text | Yes | Human-readable address displayed in the resource list. |
| `Latitude` | Number | No | Reserved for future map positioning. |
| `Longitude` | Number | No | Reserved for future map positioning. |

## Business rules for the first task

- Name, category, and at least one location are required when adding or updating a resource.
- A resource may contain multiple locations.
- Duplicate addresses are ignored within the same resource.
- New resources are always created as unverified.
- The list visibly distinguishes verified resources from resources awaiting verification.
- Deleting a resource requires confirmation.
- Editing a resource uses the same required-field validation as adding one.
- Updating a resource preserves its ID and creation time but returns it to **Pending verification** because its verified information has changed.
- Authentication and authorization are not implemented in this task. The add control will be restricted to signed-in users when account support is added.
- The role allowed to delete resources is not yet defined. The prototype exposes deletion so the workflow can be evaluated, but production authorization must be decided before release.
- Data is held in memory for this task and is lost when the application closes. Persistent storage is a separate design decision.

## Initial categories

- Food
- Shelter
- Healthcare
- Clothing
- Employment
- Transportation
- Legal Assistance
- Other

## Acceptance criteria

1. The page displays all resources currently held by the application.
2. The page displays a helpful empty state when no resources exist.
3. Selecting **Add resource** opens the entry form.
4. The form prevents submission when name, category, or address is missing.
5. A valid submission appears in the list as **Pending verification**.
6. Selecting **Delete** requests confirmation before removing the resource.
7. Cancelling either the add form or deletion leaves the resource list unchanged.
8. Each resource provides an **Edit** action that opens the form with its current values.
9. Saving valid edits updates the existing list item rather than adding a duplicate.
10. Cancelling an edit leaves the resource unchanged.
11. A user can add multiple addresses to a resource before saving it.
12. A user can remove an address from the draft without deleting the resource.
13. Every saved resource contains at least one address.
14. All saved addresses are displayed on the resource card.
