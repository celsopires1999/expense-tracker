# Expense Categories

## Purpose
Define the categories that expenses can be classified under, enabling consistent expense organization and reporting.

## Requirements

### Requirement: List categories
The system SHALL return all available categories. Any authenticated user SHALL be able to read categories.

#### Scenario: Default user lists categories
- **WHEN** a default user sends GET `/categories`
- **THEN** the response returns 200 OK with the list of categories

### Requirement: Admin creates category
The system SHALL allow Admin to create a new category.

#### Scenario: Admin creates category
- **WHEN** an Admin sends POST `/categories` with a valid name
- **THEN** the response returns 201 Created with the category ID

#### Scenario: Default user tries to create category
- **WHEN** a default user sends POST `/categories`
- **THEN** the response returns 403 Forbidden

### Requirement: Admin updates category
The system SHALL allow Admin to update an existing category's name.

#### Scenario: Admin updates category
- **WHEN** an Admin sends PUT `/categories/{id}` with a new name
- **THEN** the response returns 200 OK with the updated category

### Requirement: Admin deletes category
The system SHALL allow Admin to delete a category.

#### Scenario: Admin deletes unused category
- **WHEN** an Admin sends DELETE `/categories/{id}` and no expenses reference it
- **THEN** the response returns 204 No Content

#### Scenario: Admin deletes category in use
- **WHEN** an Admin sends DELETE `/categories/{id}` and expenses reference it
- **THEN** the response returns 409 Conflict

### Requirement: Category name uniqueness
The system SHALL enforce unique category names.

#### Scenario: Duplicate category name
- **WHEN** an Admin sends POST `/categories` with a name that already exists
- **THEN** the response returns 409 Conflict

### Requirement: Seed default categories
The system SHALL seed default categories on first migration.

#### Scenario: Default categories exist after migration
- **WHEN** the database migration runs
- **THEN** categories like "Alimentação", "Transporte", "Moradia", "Saúde", "Lazer", "Educação" SHALL exist
