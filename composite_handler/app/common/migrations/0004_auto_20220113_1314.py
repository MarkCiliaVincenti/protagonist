# Generated by Django 3.2.9 on 2022-01-13 13:14

from django.db import migrations, models
import django.db.models.deletion
import uuid


class Migration(migrations.Migration):

    dependencies = [
        ("common", "0003_auto_20220112_1454"),
    ]

    operations = [
        migrations.CreateModel(
            name="DLCSBatch",
            fields=[
                (
                    "id",
                    models.UUIDField(
                        default=uuid.uuid4,
                        editable=False,
                        primary_key=True,
                        serialize=False,
                    ),
                ),
                ("json_data", models.JSONField()),
                ("uri", models.TextField(null=True)),
                ("created_date", models.DateTimeField(auto_now_add=True)),
                ("last_updated_date", models.DateTimeField(auto_now=True)),
                (
                    "member",
                    models.ForeignKey(
                        on_delete=django.db.models.deletion.CASCADE, to="common.member"
                    ),
                ),
            ],
        ),
        migrations.DeleteModel(
            name="DlcsUri",
        ),
    ]