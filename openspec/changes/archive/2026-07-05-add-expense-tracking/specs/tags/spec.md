## ADDED Requirements

### Requirement: List tags
The system SHALL return all available tags. Any authenticated user SHALL be able to read tags.

#### Scenario: Default user lists tags
- **WHEN** a default user sends GET `/tags`
- **THEN** the response returns 200 OK with the list of tags

### Requirement: Admin creates tag
The system SHALL allow Admin to create a new global tag.

#### Scenario: Admin creates tag
- **WHEN** an Admin sends POST `/tags` with a valid name
- **THEN** the response returns 201 Created with the tag ID

#### Scenario: Default user tries to create tag
- **WHEN** a default user sends POST `/tags`
- **THEN** the response returns 403 Forbidden

### Requirement: Admin updates tag
The system SHALL allow Admin to update an existing tag's name.

#### Scenario: Admin updates tag
- **WHEN** an Admin sends PUT `/tags/{id}` with a new name
- **THEN** the response returns 200 OK with the updated tag

### Requirement: Admin deletes tag
The system SHALL allow Admin to delete a tag. Deleting a tag SHALL remove all ExpenseTag associations.

#### Scenario: Admin deletes tag
- **WHEN** an Admin sends DELETE `/tags/{id}`
- **THEN** the response returns 204 No Content
- **THEN** all expense-tag associations for that tag are also removed

### Requirement: Tag name uniqueness
The system SHALL enforce unique tag names.

#### Scenario: Duplicate tag name
- **WHEN** an Admin sends POST `/tags` with a name that already exists
- **THEN** the response returns 409 Conflict
