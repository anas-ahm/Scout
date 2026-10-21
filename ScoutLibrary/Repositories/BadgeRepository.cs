using MySqlConnector;
using ScoutLibrary.Models;

namespace ScoutLibrary.Repositories;

public class BadgeRepository : ICRUD<Badge>
{
	private readonly DBWorker _db;

	public BadgeRepository(DBWorker db)
	{
		_db = db;
	}

	public Badge Create(Badge badge)
	{
		const string query = """
		INSERT INTO badges (name, description) VALUES (@name, @description);
		SELECT LAST_INSERT_ID();
		""";

		using MySqlConnection connection = _db.Connect();

		using MySqlCommand command = new MySqlCommand(query, connection);
		command.Parameters.AddWithValue("@name", badge.Name);
		command.Parameters.AddWithValue("@description", badge.Description ?? (object)DBNull.Value);

		int newId = Convert.ToInt32(command.ExecuteScalar());
		badge.BadgeId = newId;

		return badge;
	}

	public Badge Delete(int id)
	{
		throw new NotImplementedException();
	}

	public List<Badge> GetAll()
	{
		const string query = """
		SELECT badge_id, name, description FROM badges;
		""";

		List<Badge> badges = new List<Badge>();

		using MySqlConnection connection = _db.Connect();

		using MySqlCommand command = new MySqlCommand(query, connection);
		using MySqlDataReader reader = command.ExecuteReader();

		while (reader.Read())
		{
			Badge badge = new Badge
			{
				BadgeId = reader.GetInt32("badge_id"),
				Name = reader.GetString("name"),
				Description = reader.GetString("description")
			};

			badges.Add(badge);
		}

		return badges;
	}

	public Badge GetById(int id)
	{
		throw new NotImplementedException();
	}

	public Badge Update(Badge newItem, int id)
	{
		throw new NotImplementedException();
	}
}
